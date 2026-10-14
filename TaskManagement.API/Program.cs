using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.Models.Dto;
using TaskManagement.API.Repositories;
using TaskManagement.API.Repositories.Interfaces;
using TaskManagement.API.Services;
using TaskManagement.API.Services.Interfaces;

/* HACK: Не успел доделать интеграционный тест. Как бы я поступил
 * 1. Создал проект TaskManagement.Tests (xUnit)
 * 2. Добавил в него пакеты Microsoft.AspNetCore.Mvc.Testing
 *                          Microsoft.EntityFrameworkCore.Sqlite
 * 3. В Program.cs API добавил `public partial class Program { }`, чтобы проект Tests в будущем смог собраться
 * 4. Создал тестовую БД
 *
 * 5. Завёл FakeEventPublisher : IEventPublisher — простой класс,
 *    который вместо реального RabbitMQ складывает события в ConcurrentBag<TaskCompletedEvent>.
 *    Интерфейс IEventPublisher я выделил именно для того, чтобы можно было подменить реализацию в тестах. 
 * 
 * 6. Сделал CustomWebApplicationFactory : WebApplicationFactory<Program>, 
 *    который:
 *      - подменяет строку подключения на taskdb_test; 
 *      - подменяет IEventPublisher на FakeEventPublisher; 
 *      - убирает RabbitMqHostedService, чтобы тест не пытался 
 *        подключаться к реальному RabbitMQ при старте; 
 *      - накатывает миграции на taskdb_test. 
 * 
 * 7. Написал сам тест CompleteTaskIntegrationTest (IClassFixture): 
 *      - POST /tasks { "title": "Buy milk", "priority": "High" } → 201, 
 *        запоминаю id из ответа; 
 *      - PUT /tasks/{id}/complete → 204; 
 *      - проверяю, что FakeEventPublisher получил ровно одно событие, 
 *        и что в нём: 
 *          TaskId      == созданному id, 
 *          Title       == "Buy milk", 
 *          Priority    == "High", 
 *          CompletedAt != default.
 */
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<RabbitMqHostedService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RabbitMqHostedService>());

builder.Services.AddSingleton<IEventPublisher>(sp =>
    new LazyEventPublisher(() => RabbitMqHostedService.Publisher, sp.GetRequiredService<ILogger<LazyEventPublisher>>()));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("tasks", async ([FromBody]TaskItemDto dto,
    ITaskService taskService, CancellationToken ctx) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest(new { error = "Title is required." });
    var result = await taskService.AddAsync(dto, ctx);
    return Results.Ok(result);
});

app.MapGet("tasks", async (
    ITaskService taskService, CancellationToken ctx) =>
{
    var result = await taskService.GetAllAsync(ctx);
    return Results.Ok(result);
});

app.MapPut("tasks/{id:guid}/complete", async (Guid id,  ITaskService taskService, CancellationToken ctx) =>
{
    try
    {
        var result = await taskService.CompleteAsync(id, ctx);
        return result == null ? Results.NotFound() : Results.Ok();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict();
    }
});

app.MapDelete("tasks/{id:guid}", async (Guid id, ITaskService taskService, CancellationToken ctx) =>
{
    var result = await taskService.RemoveAsync(id, ctx);
    return result == null ? Results.NotFound() : Results.Ok();
});

app.Run();

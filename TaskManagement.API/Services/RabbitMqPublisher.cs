using System.Text.Json;
using RabbitMQ.Client;
using TaskManagement.API.Models.Events;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Services;

public sealed class RabbitMqPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private const string ExchangeName = "task.events";
    private const string RoutingKey = "task.completed";

    private RabbitMqPublisher(ILogger<RabbitMqPublisher> logger, IConnection connection, IChannel channel)
    {
        _logger = logger;
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMqPublisher> CreateAsync(IConfiguration config,
        ILogger<RabbitMqPublisher> logger,
        CancellationToken ctx = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMq:Host"] ?? "localhost",
            Port = int.Parse(config["RabbitMq:Port"] ?? "5672"),
            UserName = config["RabbitMq:UserName"] ?? "guest",
            Password = config["RabbitMq:Password"] ?? "guest"
        };

        var connection = await factory.CreateConnectionAsync(ctx);
        var channel = await connection.CreateChannelAsync(cancellationToken: ctx);

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: ctx);
        
        logger.LogInformation("RabbitMQ publisher connected to {Host}", factory.HostName);
        return new RabbitMqPublisher(logger, connection, channel);
    }
    
    public async Task PublishTaskCompletedAsync(TaskCompletedEvent evt, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(evt);

        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };
        
        await _channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: RoutingKey,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);

        _logger.LogInformation("Published task.completed for {TaskId}", evt.TaskId);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_channel.IsOpen)
                await _channel.CloseAsync();
            if (_connection.IsOpen)
                await _connection.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while closing RabbitMQ connection");
        }

        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
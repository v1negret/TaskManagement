using System.Data;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Models;
using TaskManagement.API.Models.Dto;
using TaskManagement.API.Models.Events;
using TaskManagement.API.Repositories.Interfaces;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ITaskRepository taskRepository, IEventPublisher eventPublisher, ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskItemDto>> GetAllAsync(CancellationToken ctx = default)
    {
        var tasks = await _taskRepository.GetAllAsync(ctx);
        return tasks.Select(x => new TaskItemDto(x)).ToList();
    }

    public async Task<TaskItemDto> AddAsync(TaskItemDto taskDto, CancellationToken ctx = default)
    {
        var task = await _taskRepository.AddAsync(new TaskItem(taskDto), ctx);
        return new TaskItemDto(task);
    }

    public async Task<TaskItemDto?> CompleteAsync(Guid id, CancellationToken ctx = default)
    {
        var task = await _taskRepository.GetAsync(id, ctx);
        if(task == null)
            return null;
        if (task.IsCompleted)
            throw new DbUpdateConcurrencyException();
        
        task.IsCompleted =  true;
        task.CompletedAt = DateTimeOffset.UtcNow;
        
        var res = await _taskRepository.UpdateAsync(task, ctx);
        
        try
        {
            await _eventPublisher.PublishTaskCompletedAsync(new TaskCompletedEvent(
                TaskId: task.Id,
                Title: task.Title!,
                CompletedAt: task.CompletedAt.Value,
                Priority: task.Priority.ToString()), ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish task.completed for {TaskId}", task.Id);
        }
        
        return new TaskItemDto(res);
    }

    public async Task<TaskItemDto?> RemoveAsync(Guid id, CancellationToken ctx = default)
    {
        var task = await _taskRepository.RemoveAsync(id, ctx);
        if (task == null)
            return null;
        return new TaskItemDto(task);
    }
}
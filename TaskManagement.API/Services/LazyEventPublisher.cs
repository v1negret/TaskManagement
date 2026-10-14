using TaskManagement.API.Models.Events;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Services;

public class LazyEventPublisher : IEventPublisher
{
    private readonly Func<RabbitMqPublisher?> _getPublisher;
    private readonly ILogger<LazyEventPublisher> _logger;

    public LazyEventPublisher(Func<RabbitMqPublisher?> getPublisher, ILogger<LazyEventPublisher> logger)
    {
        _getPublisher = getPublisher;
        _logger = logger;
    }

    public async Task PublishTaskCompletedAsync(TaskCompletedEvent evt, CancellationToken ct = default)
    {
        var publisher = _getPublisher();
        if (publisher is null)
        {
            _logger.LogWarning("RabbitMQ unavailable, skipping task.completed for {TaskId}", evt.TaskId);
            return;
        }

        try
        {
            await publisher.PublishTaskCompletedAsync(evt, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish task.completed for {TaskId}", evt.TaskId);
        }
    }
}
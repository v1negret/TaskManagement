using TaskManagement.API.Models.Events;

namespace TaskManagement.API.Services.Interfaces;

public interface IEventPublisher
{
    Task PublishTaskCompletedAsync(TaskCompletedEvent evt, CancellationToken ct = default);
}
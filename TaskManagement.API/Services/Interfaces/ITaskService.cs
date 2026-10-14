using TaskManagement.API.Models.Dto;

namespace TaskManagement.API.Services.Interfaces;

public interface ITaskService
{
    public Task<IReadOnlyList<TaskItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<TaskItemDto> AddAsync(TaskItemDto taskDto, CancellationToken cancellationToken = default);
    public Task<TaskItemDto?> CompleteAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<TaskItemDto?> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}
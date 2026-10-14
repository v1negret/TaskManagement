using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories.Interfaces;

public interface ITaskRepository
{
    public  Task<List<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<TaskItem?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    public Task<TaskItem?> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
}
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.Models;
using TaskManagement.API.Repositories.Interfaces;

namespace TaskManagement.API.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TaskRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TaskItem>> GetAllAsync(CancellationToken ctx)
        => await _dbContext.Tasks.
            AsNoTracking()
            .ToListAsync(ctx);

    public async Task<TaskItem?> GetAsync(Guid id, CancellationToken ctx)
        =>  await _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: ctx);


    public async Task<TaskItem> AddAsync(TaskItem task, CancellationToken ctx)
    {
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(ctx);
        return task;
    }

    public async Task<TaskItem?> RemoveAsync(Guid id, CancellationToken ctx)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: ctx);
        if (task == null)
            return null;
        
        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(ctx);

        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken ctx)
    {
            _dbContext.Tasks.Update(task);
            await _dbContext.SaveChangesAsync(ctx);
            return task;
    }
}
using TaskManagement.API.Models.Dto;
using TaskManagement.API.Models.Enum;

namespace TaskManagement.API.Models;

public class TaskItem
{
    public TaskItem()
    {
        
    }
    public TaskItem(TaskItemDto dto)
    {
        Id = dto.Id;
        Title = dto.Title;
        IsCompleted = dto.IsCompleted;
        CreatedAt = dto.CreatedAt;
        CompletedAt = dto.CompletedAt;
        Priority = dto.Priority;
        Version = dto.Version;
    }
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public uint Version { get; set; }
}
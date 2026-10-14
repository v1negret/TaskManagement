using TaskManagement.API.Models.Enum;

namespace TaskManagement.API.Models.Dto;

public class TaskItemDto
{
    public TaskItemDto()
    {
        
    }
    public TaskItemDto(TaskItem item)
    {
        Id = item.Id;
        Title = item.Title;
        IsCompleted =  item.IsCompleted;
        Priority = item.Priority;
        CreatedAt = item.CreatedAt;
        CompletedAt = item.CompletedAt;
        Version = item.Version;
    }
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public uint Version { get; set; }
}
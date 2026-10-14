namespace TaskManagement.API.Models.Events;

public record TaskCompletedEvent(
    Guid TaskId,
    string Title,
    DateTimeOffset CompletedAt,
    string Priority);
namespace PersonalizedJira.Application.DTOs;

public sealed record TaskDto(
    Guid Id,
    Guid WorkspaceId,
    string Title,
    string Description,
    string Assignee,
    DateOnly DueDate,
    string Label,
    string Status);

public sealed record MoveTaskRequest(string Status);

public sealed record SearchResponse(string Query, IReadOnlyList<TaskDto> Tasks);

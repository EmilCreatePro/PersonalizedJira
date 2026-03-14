namespace PersonalizedJira.Domain.Entities;

public sealed class JiraTask
{
    public required Guid Id { get; init; }
    public required Guid WorkspaceId { get; init; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Assignee { get; set; }
    public required DateOnly DueDate { get; set; }
    public required string Label { get; set; }
    public required string Status { get; set; }
}

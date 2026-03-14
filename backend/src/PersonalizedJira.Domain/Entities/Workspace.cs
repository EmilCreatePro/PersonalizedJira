namespace PersonalizedJira.Domain.Entities;

public sealed class Workspace
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}

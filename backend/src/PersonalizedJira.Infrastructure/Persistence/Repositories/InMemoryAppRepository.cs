using PersonalizedJira.Application.DTOs;
using PersonalizedJira.Application.Interfaces;

namespace PersonalizedJira.Infrastructure.Persistence.Repositories;

public sealed class InMemoryAppRepository : IAppRepository
{
    private readonly List<WorkspaceDto> _workspaces =
    [
        new WorkspaceDto(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Personal Workspace"),
        new WorkspaceDto(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Team Roadmap")
    ];

    private readonly List<TaskDto> _tasks =
    [
        new TaskDto(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Set up Auth flow",
            "Create login and register pages with validation.",
            "Emil",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)),
            "frontend",
            "todo"),
        new TaskDto(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Kanban API endpoint",
            "Expose tasks grouped by status.",
            "Alex",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6)),
            "backend",
            "in-progress"),
        new TaskDto(
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Wire SignalR client",
            "Connect frontend to realtime hub.",
            "Emil",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            "realtime",
            "done")
    ];

    public AuthResponse Login(LoginRequest request)
    {
        var displayName = request.Email.Split('@', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Guest";
        return new AuthResponse("dev-token-123", displayName);
    }

    public IReadOnlyList<WorkspaceDto> GetWorkspaces() => _workspaces;

    public IReadOnlyList<TaskDto> GetTasksByWorkspace(Guid workspaceId) =>
        _tasks.Where(t => t.WorkspaceId == workspaceId).ToList();

    public IReadOnlyList<TaskDto> SearchTasks(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return _tasks;
        }

        return _tasks.Where(t =>
                t.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                || t.Description.Contains(query, StringComparison.OrdinalIgnoreCase)
                || t.Label.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public IReadOnlyList<TaskDto> FilterTasks(string? assignee, string? label)
    {
        IEnumerable<TaskDto> result = _tasks;

        if (!string.IsNullOrWhiteSpace(assignee))
        {
            result = result.Where(t => t.Assignee.Equals(assignee, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(label))
        {
            result = result.Where(t => t.Label.Equals(label, StringComparison.OrdinalIgnoreCase));
        }

        return result.ToList();
    }

    public TaskDto? MoveTask(Guid taskId, string status)
    {
        var index = _tasks.FindIndex(t => t.Id == taskId);
        if (index < 0)
        {
            return null;
        }

        var current = _tasks[index];
        var updated = current with { Status = status };
        _tasks[index] = updated;

        return updated;
    }
}

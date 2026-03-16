using PersonalizedJira.Application.DTOs;
using PersonalizedJira.Application.Interfaces;

namespace PersonalizedJira.Infrastructure.Persistence.Repositories;

public sealed class InMemoryAppRepository : IAppRepository
{
    private readonly object _sync = new();

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

    private readonly Dictionary<string, string> _users = new(StringComparer.OrdinalIgnoreCase)
    {
        ["emil@example.com"] = "123456"
    };

    private readonly HashSet<string> _issuedTokens = new(StringComparer.Ordinal);

    public AuthResponse Register(LoginRequest request)
    {
        var email = request.Email.Trim();

        lock (_sync)
        {
            if (_users.ContainsKey(email))
            {
                throw new InvalidOperationException("User already exists.");
            }

            _users[email] = request.Password;
            return IssueAuthResponse(email);
        }
    }

    public AuthResponse Login(LoginRequest request)
    {
        var email = request.Email.Trim();

        lock (_sync)
        {
            if (!_users.TryGetValue(email, out var storedPassword) || storedPassword != request.Password)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            return IssueAuthResponse(email);
        }
    }

    public bool IsTokenValid(string token)
    {
        lock (_sync)
        {
            return _issuedTokens.Contains(token);
        }
    }

    public IReadOnlyList<WorkspaceDto> GetWorkspaces()
    {
        lock (_sync)
        {
            return _workspaces.ToList();
        }
    }

    public IReadOnlyList<TaskDto> GetTasksByWorkspace(Guid workspaceId)
    {
        lock (_sync)
        {
            return _tasks.Where(t => t.WorkspaceId == workspaceId).ToList();
        }
    }

    public IReadOnlyList<TaskDto> SearchTasks(string query)
    {
        lock (_sync)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return _tasks.ToList();
            }

            return _tasks.Where(t =>
                    t.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || t.Description.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || t.Label.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    public IReadOnlyList<TaskDto> FilterTasks(string? assignee, string? label)
    {
        lock (_sync)
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
    }

    public TaskDto? MoveTask(Guid taskId, string status)
    {
        lock (_sync)
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

    public TaskDto CreateTask(Guid workspaceId, CreateTaskRequest request)
    {
        lock (_sync)
        {
            var created = new TaskDto(
                Guid.NewGuid(),
                workspaceId,
                request.Title.Trim(),
                request.Description.Trim(),
                request.Assignee.Trim(),
                request.DueDate,
                request.Label.Trim(),
                request.Status.Trim().ToLowerInvariant());

            _tasks.Add(created);
            return created;
        }
    }

    public bool DeleteTask(Guid taskId)
    {
        lock (_sync)
        {
            var index = _tasks.FindIndex(t => t.Id == taskId);
            if (index < 0)
            {
                return false;
            }

            _tasks.RemoveAt(index);
            return true;
        }
    }

    private AuthResponse IssueAuthResponse(string email)
    {
        var displayName = email.Split('@', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Guest";
        var token = $"dev-{Guid.NewGuid():N}";
        _issuedTokens.Add(token);
        return new AuthResponse(token, displayName);
    }
}

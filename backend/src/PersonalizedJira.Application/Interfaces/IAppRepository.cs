using PersonalizedJira.Application.DTOs;

namespace PersonalizedJira.Application.Interfaces;

public interface IAppRepository
{
    AuthResponse Login(LoginRequest request);
    IReadOnlyList<WorkspaceDto> GetWorkspaces();
    IReadOnlyList<TaskDto> GetTasksByWorkspace(Guid workspaceId);
    IReadOnlyList<TaskDto> SearchTasks(string query);
    IReadOnlyList<TaskDto> FilterTasks(string? assignee, string? label);
    TaskDto? MoveTask(Guid taskId, string status);
}

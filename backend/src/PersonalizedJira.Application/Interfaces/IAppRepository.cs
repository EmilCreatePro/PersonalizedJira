using PersonalizedJira.Application.DTOs;

namespace PersonalizedJira.Application.Interfaces;

public interface IAppRepository
{
    AuthResponse Register(LoginRequest request);
    AuthResponse Login(LoginRequest request);
    bool IsTokenValid(string token);
    IReadOnlyList<WorkspaceDto> GetWorkspaces();
    IReadOnlyList<TaskDto> GetTasksByWorkspace(Guid workspaceId);
    IReadOnlyList<TaskDto> SearchTasks(string query);
    IReadOnlyList<TaskDto> FilterTasks(string? assignee, string? label);
    TaskDto? MoveTask(Guid taskId, string status);
    TaskDto CreateTask(Guid workspaceId, CreateTaskRequest request);
    bool DeleteTask(Guid taskId);
}

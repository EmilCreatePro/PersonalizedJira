using PersonalizedJira.Application.DTOs;
using PersonalizedJira.Infrastructure.Persistence.Repositories;
using Xunit;

namespace PersonalizedJira.Infrastructure.Tests;

public sealed class InMemoryAppRepositoryTests
{
    private readonly InMemoryAppRepository _repository = new();

    [Fact]
    public void Login_UsesEmailPrefixAsDisplayName()
    {
        var request = new LoginRequest("emil@example.com", "secret");

        var response = _repository.Login(request);

        Assert.Equal("dev-token-123", response.Token);
        Assert.Equal("emil", response.DisplayName);
    }

    [Fact]
    public void GetWorkspaces_ReturnsSeededWorkspaces()
    {
        var workspaces = _repository.GetWorkspaces();

        Assert.Equal(2, workspaces.Count);
        Assert.Contains(workspaces, w => w.Name == "Personal Workspace");
        Assert.Contains(workspaces, w => w.Name == "Team Roadmap");
    }

    [Fact]
    public void SearchTasks_EmptyQuery_ReturnsAllTasks()
    {
        var tasks = _repository.SearchTasks(string.Empty);

        Assert.Equal(3, tasks.Count);
    }

    [Fact]
    public void SearchTasks_MatchesByTitleDescriptionOrLabel_IgnoringCase()
    {
        var byTitle = _repository.SearchTasks("auth");
        var byDescription = _repository.SearchTasks("grouped by status");
        var byLabel = _repository.SearchTasks("REALTIME");

        Assert.Single(byTitle);
        Assert.Equal("Set up Auth flow", byTitle[0].Title);

        Assert.Single(byDescription);
        Assert.Equal("Kanban API endpoint", byDescription[0].Title);

        Assert.Single(byLabel);
        Assert.Equal("Wire SignalR client", byLabel[0].Title);
    }

    [Fact]
    public void FilterTasks_AppliesAssigneeAndLabelFilters()
    {
        var tasks = _repository.FilterTasks("Emil", "frontend");

        Assert.Single(tasks);
        Assert.Equal("Set up Auth flow", tasks[0].Title);
    }

    [Fact]
    public void MoveTask_ExistingTask_UpdatesStatusAndReturnsUpdatedTask()
    {
        var taskId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var updated = _repository.MoveTask(taskId, "done");

        Assert.NotNull(updated);
        Assert.Equal("done", updated!.Status);

        var byWorkspace = _repository.GetTasksByWorkspace(updated.WorkspaceId);
        Assert.Contains(byWorkspace, t => t.Id == taskId && t.Status == "done");
    }

    [Fact]
    public void MoveTask_UnknownTask_ReturnsNull()
    {
        var result = _repository.MoveTask(Guid.NewGuid(), "done");

        Assert.Null(result);
    }
}

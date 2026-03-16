using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace PersonalizedJira.Api.Tests;

public sealed class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenEmailOrPasswordIsMissing()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "",
            Password = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsTokenAndDisplayName_WhenPayloadIsValid()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "emil@example.com",
            Password = "123456"
        });

        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;

        Assert.Equal("dev-token-123", root.GetProperty("token").GetString());
        Assert.Equal("emil", root.GetProperty("displayName").GetString());
    }

    [Fact]
    public async Task Search_ReturnsExpectedShape_WithQueryAndTasks()
    {
        var response = await _client.GetAsync("/api/search?q=auth");

        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;

        Assert.True(root.TryGetProperty("query", out var query));
        Assert.True(root.TryGetProperty("tasks", out var tasks));
        Assert.Equal("auth", query.GetString());
        Assert.Equal(JsonValueKind.Array, tasks.ValueKind);
        Assert.True(tasks.GetArrayLength() >= 1);
    }

    [Fact]
    public async Task MoveTask_UpdatesStatus_WhenTaskExists()
    {
        var existingTaskId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

        var response = await _client.PostAsJsonAsync($"/api/tasks/{existingTaskId}/move", new
        {
            Status = "Done"
        });

        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;

        Assert.Equal(existingTaskId, root.GetProperty("id").GetString());
        Assert.Equal("done", root.GetProperty("status").GetString());
    }

    [Fact]
    public async Task MoveTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync($"/api/tasks/{Guid.NewGuid()}/move", new
        {
            Status = "done"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_ReturnsSuccessForValidPayload_AndBadRequestForInvalidPayload()
    {
        var workspaceId = "11111111-1111-1111-1111-111111111111";

        var successResponse = await _client.PostAsJsonAsync($"/api/boards/{workspaceId}/tasks", new
        {
            Title = "Interview create task",
            Description = "Create endpoint integration test",
            Assignee = "Emil",
            DueDate = "2026-03-20",
            Label = "testing",
            Status = "todo"
        });

        successResponse.EnsureSuccessStatusCode();

        using (var successJson = JsonDocument.Parse(await successResponse.Content.ReadAsStringAsync()))
        {
            var created = successJson.RootElement;
            Assert.Equal(workspaceId, created.GetProperty("workspaceId").GetString());
            Assert.Equal("Interview create task", created.GetProperty("title").GetString());
            Assert.Equal("todo", created.GetProperty("status").GetString());
        }

        var invalidResponse = await _client.PostAsJsonAsync($"/api/boards/{workspaceId}/tasks", new
        {
            Title = "",
            Description = "Missing title",
            Assignee = "Emil",
            DueDate = "2026-03-20",
            Label = "testing",
            Status = "todo"
        });

        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNoContentForExistingTask_AndNotFoundForMissingTask()
    {
        var workspaceId = "11111111-1111-1111-1111-111111111111";

        var createResponse = await _client.PostAsJsonAsync($"/api/boards/{workspaceId}/tasks", new
        {
            Title = "Task to delete",
            Description = "Delete endpoint integration test",
            Assignee = "Emil",
            DueDate = "2026-03-21",
            Label = "testing",
            Status = "todo"
        });

        createResponse.EnsureSuccessStatusCode();

        using var createdJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var createdId = createdJson.RootElement.GetProperty("id").GetString();
        Assert.False(string.IsNullOrWhiteSpace(createdId));

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{createdId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var deleteMissingResponse = await _client.DeleteAsync($"/api/tasks/{createdId}");
        Assert.Equal(HttpStatusCode.NotFound, deleteMissingResponse.StatusCode);
    }
}

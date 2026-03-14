using Microsoft.AspNetCore.SignalR;
using PersonalizedJira.Api.Hubs;
using PersonalizedJira.Application.DTOs;
using PersonalizedJira.Application.Interfaces;
using PersonalizedJira.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IAppRepository, InMemoryAppRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("frontend");

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "PersonalizedJira.Api" }));

app.MapPost("/api/auth/login", (LoginRequest request, IAppRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Email and password are required." });
    }

    var auth = repository.Login(request);
    return Results.Ok(auth);
});

app.MapGet("/api/workspaces", (IAppRepository repository) => Results.Ok(repository.GetWorkspaces()));

app.MapGet("/api/boards/{workspaceId:guid}", (Guid workspaceId, IAppRepository repository) =>
{
    var tasks = repository.GetTasksByWorkspace(workspaceId);
    return Results.Ok(tasks);
});

app.MapGet("/api/search", (string? q, IAppRepository repository) =>
{
    var tasks = repository.SearchTasks(q ?? string.Empty);
    return Results.Ok(new SearchResponse(q ?? string.Empty, tasks));
});

app.MapGet("/api/tasks/filter", (string? assignee, string? label, IAppRepository repository) =>
{
    var tasks = repository.FilterTasks(assignee, label);
    return Results.Ok(tasks);
});

app.MapPost("/api/tasks/{taskId:guid}/move", async (Guid taskId, MoveTaskRequest request, IAppRepository repository, IHubContext<UpdatesHub> hubContext) =>
{
    if (string.IsNullOrWhiteSpace(request.Status))
    {
        return Results.BadRequest(new { message = "Target status is required." });
    }

    var updated = repository.MoveTask(taskId, request.Status.Trim().ToLowerInvariant());
    if (updated is null)
    {
        return Results.NotFound(new { message = "Task not found." });
    }

    await hubContext.Clients.All.SendAsync("taskMoved", new
    {
        updated.Id,
        updated.Status,
        Message = $"Task '{updated.Title}' moved to {updated.Status}."
    });

    return Results.Ok(updated);
});

app.MapHub<UpdatesHub>("/hubs/updates");

app.Run();

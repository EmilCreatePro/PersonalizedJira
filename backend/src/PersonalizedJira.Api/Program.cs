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

RouteHandlerBuilder RequireAuth(RouteHandlerBuilder builder)
{
    return builder.AddEndpointFilter(async (context, next) =>
    {
        var request = context.HttpContext.Request;
        var header = request.Headers.Authorization.ToString();
        const string bearerPrefix = "Bearer ";

        if (!header.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Results.Unauthorized();
        }

        var token = header[bearerPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.Unauthorized();
        }

        var repository = context.HttpContext.RequestServices.GetRequiredService<IAppRepository>();
        if (!repository.IsTokenValid(token))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    });
}

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

    try
    {
        var auth = repository.Login(request);
        return Results.Ok(auth);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

app.MapPost("/api/auth/register", (LoginRequest request, IAppRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Email and password are required." });
    }

    try
    {
        var auth = repository.Register(request);
        return Results.Ok(auth);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
});

RequireAuth(app.MapGet("/api/workspaces", (IAppRepository repository) => Results.Ok(repository.GetWorkspaces())));

RequireAuth(app.MapGet("/api/boards/{workspaceId:guid}", (Guid workspaceId, IAppRepository repository) =>
{
    var tasks = repository.GetTasksByWorkspace(workspaceId);
    return Results.Ok(tasks);
}));

RequireAuth(app.MapGet("/api/search", (string? q, IAppRepository repository) =>
{
    var tasks = repository.SearchTasks(q ?? string.Empty);
    return Results.Ok(new SearchResponse(q ?? string.Empty, tasks));
}));

RequireAuth(app.MapGet("/api/tasks/filter", (string? assignee, string? label, IAppRepository repository) =>
{
    var tasks = repository.FilterTasks(assignee, label);
    return Results.Ok(tasks);
}));

RequireAuth(app.MapPost("/api/boards/{workspaceId:guid}/tasks", async (Guid workspaceId, CreateTaskRequest request, IAppRepository repository, IHubContext<UpdatesHub> hubContext) =>
{
    if (string.IsNullOrWhiteSpace(request.Title)
        || string.IsNullOrWhiteSpace(request.Description)
        || string.IsNullOrWhiteSpace(request.Assignee)
        || string.IsNullOrWhiteSpace(request.Label)
        || string.IsNullOrWhiteSpace(request.Status))
    {
        return Results.BadRequest(new { message = "Title, description, assignee, label and status are required." });
    }

    var created = repository.CreateTask(workspaceId, request);

    await hubContext.Clients.All.SendAsync("taskCreated", new
    {
        Task = created,
        Message = $"Task '{created.Title}' created."
    });

    return Results.Ok(created);
}));

RequireAuth(app.MapPost("/api/tasks/{taskId:guid}/move", async (Guid taskId, MoveTaskRequest request, IAppRepository repository, IHubContext<UpdatesHub> hubContext) =>
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
}));

RequireAuth(app.MapDelete("/api/tasks/{taskId:guid}", async (Guid taskId, IAppRepository repository, IHubContext<UpdatesHub> hubContext) =>
{
    var deleted = repository.DeleteTask(taskId);
    if (!deleted)
    {
        return Results.NotFound(new { message = "Task not found." });
    }

    await hubContext.Clients.All.SendAsync("taskDeleted", new
    {
        Id = taskId,
        Message = "Task deleted."
    });

    return Results.NoContent();
}));

app.MapHub<UpdatesHub>("/hubs/updates");

app.Run();

public partial class Program;

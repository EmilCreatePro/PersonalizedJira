# Backend

C# .NET backend for authentication, workspace/board/task management, search/filter, and real-time collaboration.

## Suggested Projects

- PersonalizedJira.Api
- PersonalizedJira.Application
- PersonalizedJira.Domain
- PersonalizedJira.Infrastructure
- Test projects under backend/tests

## Local Run

1. Build:

```powershell
dotnet build .\src\PersonalizedJira.Api\PersonalizedJira.Api.csproj
```

2. Run API on port 5000:

```powershell
dotnet run --project .\src\PersonalizedJira.Api\PersonalizedJira.Api.csproj --urls http://localhost:5000
```

3. Open Swagger:

- http://localhost:5000/swagger

## Seeded Endpoints

- `POST /api/auth/login`
- `GET /api/workspaces`
- `GET /api/boards/{workspaceId}`
- `GET /api/search?q=...`
- `GET /api/tasks/filter?assignee=...&label=...`
- `POST /api/tasks/{taskId}/move`
- `GET /api/health`

## Realtime

- SignalR hub: `GET/WS /hubs/updates`
- Event emitted when moving tasks: `taskMoved`

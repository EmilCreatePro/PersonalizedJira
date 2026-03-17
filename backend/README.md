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

### New to SignalR?

If you browse to `http://localhost:5000/hubs/updates` and see **"Connection ID required"**, that is expected.

- This route is a SignalR endpoint, not a normal HTML page.
- It must be used by a SignalR client (for example, the frontend app).

You can verify the hub is reachable by calling negotiate:

```powershell
Invoke-RestMethod -Method Post "http://localhost:5000/hubs/updates/negotiate?negotiateVersion=1"
```

Successful output includes values like `connectionId` and `availableTransports`.

## Troubleshooting (First Run Setup Check)

If realtime or API calls fail, verify these basics:

1. Backend is up on port 5000:

```powershell
Invoke-RestMethod "http://localhost:5000/api/health"
```

Expected result includes `status = ok`.

2. CORS origin is correct:

- Backend allows frontend origin `http://localhost:5173`.
- If frontend runs on another port, update backend CORS settings.

3. Frontend points to the correct backend URL:

- Frontend API and SignalR base URL should be `http://localhost:5000`.
- If backend port changes, update frontend config.

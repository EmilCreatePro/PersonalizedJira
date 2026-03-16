# PersonalizedJira

PersonalizedJira is a full-stack interview project with:

- Frontend: React + TypeScript + Vite
- Backend: ASP.NET Core (.NET 8, minimal APIs)
- Realtime: SignalR (`/hubs/updates`)
- Automated CI: GitHub Actions for backend and frontend

## Tech Stack

- .NET 8
- React 18
- TypeScript 5
- Vite
- xUnit

## Prerequisites

- .NET SDK 8.x
- Node.js 20+
- npm 10+

On Windows PowerShell, use `npm.cmd` if `npm` is blocked by execution policy.

## Local Run

From repository root, open two terminals.

### Terminal 1: Backend

```powershell
Set-Location .\backend
dotnet run --project .\src\PersonalizedJira.Api\PersonalizedJira.Api.csproj --urls http://localhost:5000
```

### Terminal 2: Frontend

```powershell
Set-Location .\frontend
npm.cmd install
npm.cmd run dev
```

### URLs

- Frontend: http://localhost:5173
- Backend Swagger: http://localhost:5000/swagger
- SignalR Hub: http://localhost:5000/hubs/updates

## Local Testing

From repository root:

### Run all tests

```powershell
dotnet test .\PersonalizedJira.sln
```

### Run infrastructure unit tests only

```powershell
dotnet test .\backend\tests\PersonalizedJira.Infrastructure.Tests\PersonalizedJira.Infrastructure.Tests.csproj
```

### Run API integration tests only

```powershell
dotnet test .\backend\tests\PersonalizedJira.Api.Tests\PersonalizedJira.Api.Tests.csproj
```

### Optional: coverage collection

```powershell
dotnet test .\PersonalizedJira.sln --collect:"XPlat Code Coverage"
```

## API Endpoints (Current Baseline)

- `GET /api/health`
- `POST /api/auth/login`
- `GET /api/workspaces`
- `GET /api/boards/{workspaceId}`
- `GET /api/search?q=...`
- `GET /api/tasks/filter?assignee=...&label=...`
- `POST /api/tasks/{taskId}/move`

## CI/CD (GitHub Actions)

Workflows:

- Backend: [.github/workflows/backend-ci-cd.yml](.github/workflows/backend-ci-cd.yml)
- Frontend: [.github/workflows/frontend-ci-cd.yml](.github/workflows/frontend-ci-cd.yml)

Behavior:

- Backend workflow runs on push/PR to `main` when backend files or solution/workflow files change.
- Frontend workflow runs on push/PR to `main` when frontend or frontend workflow files change.
- Backend pipeline restores, builds, and tests the solution.
- Frontend pipeline installs dependencies and builds the app.

## Recommended Branch Protection

For `main`, require status checks before merge:

- `Backend CI/CD / build-test`
- `Frontend CI/CD / build`

## Demo Flow for Inteview (Notes For Myself)

1. Register a new user (or login with seeded user emil@example.com / 123456).
2. Show authenticated access to workspaces and board tasks.
3. Search and filter tasks.
4. Move a task and mention SignalR event emission.
5. Logout and show protected endpoints are no longer accessible.
6. Show tests and CI passing.

## Project Structure

Key folders:

- `backend/src`: API, application, domain, infrastructure
- `backend/tests`: API integration and infrastructure unit tests
- `frontend/src`: app/features/components/services
- `.github/workflows`: CI pipelines
- `docs`: architecture and API notes

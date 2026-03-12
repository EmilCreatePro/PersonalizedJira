# PersonalizedJira Monorepo Structure

This workspace is organized for a React (TypeScript) frontend and a C# .NET backend with real-time collaboration and CI/CD.

## Folder Structure

```text
PersonalizedJira/
|-- .github/
|   `-- workflows/
|       |-- backend-ci-cd.yml
|       `-- frontend-ci-cd.yml
|-- .vscode/
|-- backend/
|   |-- src/
|   |   |-- PersonalizedJira.Api/
|   |   |   |-- Controllers/
|   |   |   |-- Extensions/
|   |   |   |-- Hubs/
|   |   |   `-- Middleware/
|   |   |-- PersonalizedJira.Application/
|   |   |   |-- DTOs/
|   |   |   |-- Features/
|   |   |   |   |-- Auth/
|   |   |   |   |-- Boards/
|   |   |   |   |-- Search/
|   |   |   |   |-- Tasks/
|   |   |   |   `-- Workspaces/
|   |   |   `-- Interfaces/
|   |   |-- PersonalizedJira.Domain/
|   |   |   |-- Entities/
|   |   |   |-- Enums/
|   |   |   |-- Events/
|   |   |   `-- ValueObjects/
|   |   `-- PersonalizedJira.Infrastructure/
|   |       |-- Identity/
|   |       |-- Persistence/
|   |       |   |-- Configurations/
|   |       |   |-- Migrations/
|   |       |   `-- Repositories/
|   |       |-- Realtime/
|   |       `-- Services/
|   `-- tests/
|       |-- PersonalizedJira.Api.Tests/
|       |-- PersonalizedJira.Application.Tests/
|       `-- PersonalizedJira.Infrastructure.Tests/
|-- docs/
|   |-- api/
|   `-- architecture/
|-- frontend/
|   |-- public/
|   |-- src/
|   |   |-- app/
|   |   |-- components/
|   |   |-- features/
|   |   |   |-- auth/
|   |   |   |   |-- components/
|   |   |   |   `-- pages/
|   |   |   |-- boards/
|   |   |   |   `-- components/
|   |   |   |-- dashboard/
|   |   |   |   `-- pages/
|   |   |   |-- search/
|   |   |   |   `-- components/
|   |   |   |-- tasks/
|   |   |   |   `-- components/
|   |   |   `-- workspaces/
|   |   |       `-- components/
|   |   |-- services/
|   |   |   |-- api/
|   |   |   `-- realtime/
|   |   |-- store/
|   |   |-- styles/
|   |   |-- types/
|   |   `-- utils/
|   `-- tests/
|-- infra/
|   |-- docker/
|   `-- k8s/
|-- scripts/
|   |-- backend/
|   |-- deploy/
|   `-- frontend/
`-- shared/
    `-- contracts/
        |-- api/
        `-- realtime/
```

## Purpose By Layer

- `frontend/`: React UI (auth, dashboard, kanban board, task cards, filtering/search).
- `backend/`: .NET API, application logic, domain, infrastructure, SignalR hubs for real-time events.
- `shared/contracts/`: Shared request/response and real-time event contracts.
- `.github/workflows/`: Frontend and backend CI/CD pipelines.
- `infra/`: Container and cluster deployment assets.
- `docs/`: Architecture and API documentation.
- `scripts/`: Automation scripts for local/dev/prod flows.

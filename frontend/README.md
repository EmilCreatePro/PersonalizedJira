# Frontend

React + TypeScript web app.

## Key Areas

- Auth views (login/register)
- Dashboard (workspace list)
- Kanban board view
- Task cards with assignee, due date, labels
- Live search and filtering
- Real-time updates via WebSocket/SignalR client

## Local Run

1. Install dependencies:

```powershell
npm.cmd install
```

2. Start dev server:

```powershell
npm.cmd run dev
```

3. Open browser:

- http://localhost:5173

## Setup Validation Checklist

- Auth panel logs in against backend (`/api/auth/login`).
- Dashboard loads workspace list from `/api/workspaces`.
- Kanban board loads tasks per workspace from `/api/boards/{workspaceId}`.
- Search panel queries `/api/search`.
- Filter panel queries `/api/tasks/filter`.
- Moving a task posts to `/api/tasks/{taskId}/move`.
- Realtime feed updates from SignalR hub `/hubs/updates`.

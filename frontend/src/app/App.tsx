import { useEffect, useMemo, useState, type FormEvent } from "react";
import { AuthPanel } from "../features/auth/components/AuthPanel";
import { DashboardPage } from "../features/dashboard/pages/DashboardPage";
import { KanbanBoard } from "../features/boards/components/KanbanBoard";
import { SearchPanel } from "../features/search/components/SearchPanel";
import {
  createTask,
  deleteTask,
  filterTasks,
  getBoard,
  getHealth,
  getWorkspaces,
  login,
  moveTask,
  searchTasks
} from "../services/api/client";
import { connectUpdatesHub } from "../services/realtime/updatesClient";
import type { CreateTaskInput, TaskItem, Workspace } from "../types/models";

export default function App() {
  const [displayName, setDisplayName] = useState<string | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [healthStatus, setHealthStatus] = useState("checking...");
  const [workspaces, setWorkspaces] = useState<Workspace[]>([]);
  const [selectedWorkspace, setSelectedWorkspace] = useState<Workspace | null>(null);
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [feed, setFeed] = useState<string[]>([]);
  const [realtimeReady, setRealtimeReady] = useState(false);
  const [pingRealtime, setPingRealtime] = useState<null | (() => Promise<void>)>(null);
  const [createTaskForm, setCreateTaskForm] = useState<CreateTaskInput>({
    title: "",
    description: "",
    assignee: "",
    dueDate: "",
    label: "",
    status: "todo"
  });

  useEffect(() => {
    getHealth()
      .then((health) => setHealthStatus(`${health.service}: ${health.status}`))
      .catch(() => setHealthStatus("backend unreachable"));

    let stopConnection: (() => Promise<void>) | null = null;

    connectUpdatesHub({
      onTaskMoved: (event) => {
        setFeed((current) => [event.message, ...current].slice(0, 5));
      },
      onPong: (message) => {
        setFeed((current) => [message, ...current].slice(0, 5));
      }
    })
      .then((hub) => {
        setRealtimeReady(true);
        setPingRealtime(() => hub.ping);
        stopConnection = hub.stop;
      })
      .catch(() => {
        setRealtimeReady(false);
      });

    return () => {
      if (stopConnection) {
        void stopConnection();
      }
    };
  }, []);

  const visibleTasks = useMemo(
    () => tasks.filter((task) => !selectedWorkspace || task.workspaceId === selectedWorkspace.id),
    [tasks, selectedWorkspace]
  );

  const handleLogin = async (email: string, password: string) => {
    const auth = await login(email, password);
    setDisplayName(auth.displayName);
    setToken(auth.token);

    const loadedWorkspaces = await getWorkspaces();
    setWorkspaces(loadedWorkspaces);

    if (loadedWorkspaces.length > 0) {
      const first = loadedWorkspaces[0];
      setSelectedWorkspace(first);
      setTasks(await getBoard(first.id));
    }
  };

  const handleSelectWorkspace = async (workspace: Workspace) => {
    setSelectedWorkspace(workspace);
    setTasks(await getBoard(workspace.id));
  };

  const handleSearch = async (query: string) => {
    const result = await searchTasks(query);
    setTasks(result.tasks);
  };

  const handleFilter = async (assignee: string, label: string) => {
    setTasks(await filterTasks(assignee.trim(), label.trim()));
  };

  const handleMoveTask = async (task: TaskItem, targetStatus?: string) => {
    const next = targetStatus
      ?? (task.status === "todo" ? "in-progress" : task.status === "in-progress" ? "done" : "todo");
    const updated = await moveTask(task.id, next);

    setTasks((current) => current.map((item) => (item.id === updated.id ? updated : item)));
  };

  const handleCreateTask = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedWorkspace) {
      return;
    }

    const created = await createTask(selectedWorkspace.id, createTaskForm);
    setTasks((current) => [...current, created]);
    setCreateTaskForm({
      title: "",
      description: "",
      assignee: "",
      dueDate: "",
      label: "",
      status: "todo"
    });
  };

  const handleDeleteTask = async (taskId: string) => {
    await deleteTask(taskId);
    setTasks((current) => current.filter((task) => task.id !== taskId));
  };

  return (
    <main className="app-shell">
      <header className="hero">
        <h1>PersonalizedJira Setup Check</h1>
        <p>Backend status: {healthStatus}</p>
        <p>Realtime: {realtimeReady ? "connected" : "disconnected"}</p>
        <div className="hero-meta">
          <span>{displayName ? `Signed in as ${displayName}` : "Not signed in"}</span>
          <span>{token ? "Token received" : "No token yet"}</span>
          <button type="button" disabled={!pingRealtime} onClick={() => pingRealtime?.()}>
            Ping Realtime
          </button>
        </div>
      </header>

      <AuthPanel onLogin={handleLogin} />

      <DashboardPage
        workspaces={workspaces}
        selectedWorkspaceId={selectedWorkspace?.id ?? null}
        onSelectWorkspace={handleSelectWorkspace}
      />

      <SearchPanel onSearch={handleSearch} onFilter={handleFilter} />

      <section className="panel">
        <h2>Add Task</h2>
        <p className="muted">
          {selectedWorkspace
            ? `Adding to workspace: ${selectedWorkspace.name}`
            : "Select a workspace first to create a task."}
        </p>
        <form className="task-create-grid" onSubmit={handleCreateTask}>
          <label>
            Title
            <input
              required
              value={createTaskForm.title}
              onChange={(event) => setCreateTaskForm((current) => ({ ...current, title: event.target.value }))}
            />
          </label>

          <label>
            Description
            <input
              required
              value={createTaskForm.description}
              onChange={(event) => setCreateTaskForm((current) => ({ ...current, description: event.target.value }))}
            />
          </label>

          <label>
            Assignee
            <input
              required
              value={createTaskForm.assignee}
              onChange={(event) => setCreateTaskForm((current) => ({ ...current, assignee: event.target.value }))}
            />
          </label>

          <label>
            Due Date
            <input
              required
              type="date"
              value={createTaskForm.dueDate}
              onChange={(event) => setCreateTaskForm((current) => ({ ...current, dueDate: event.target.value }))}
            />
          </label>

          <label>
            Label
            <input
              required
              value={createTaskForm.label}
              onChange={(event) => setCreateTaskForm((current) => ({ ...current, label: event.target.value }))}
            />
          </label>

          <label>
            Status
            <select
              value={createTaskForm.status}
              onChange={(event) =>
                setCreateTaskForm((current) => ({
                  ...current,
                  status: event.target.value as CreateTaskInput["status"]
                }))
              }
            >
              <option value="todo">todo</option>
              <option value="in-progress">in-progress</option>
              <option value="done">done</option>
            </select>
          </label>

          <button type="submit" disabled={!selectedWorkspace}>
            Add Task
          </button>
        </form>
      </section>

      <KanbanBoard tasks={visibleTasks} onMoveTask={handleMoveTask} onDeleteTask={handleDeleteTask} />

      <section className="panel">
        <h2>Realtime Feed</h2>
        <ul className="feed-list">
          {feed.length === 0 ? <li>No events yet. Move a task or ping realtime.</li> : null}
          {feed.map((entry, index) => (
            <li key={`${entry}-${index}`}>{entry}</li>
          ))}
        </ul>
      </section>
    </main>
  );
}

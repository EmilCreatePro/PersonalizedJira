import { useEffect, useMemo, useState } from "react";
import { AuthPanel } from "../features/auth/components/AuthPanel";
import { DashboardPage } from "../features/dashboard/pages/DashboardPage";
import { KanbanBoard } from "../features/boards/components/KanbanBoard";
import { SearchPanel } from "../features/search/components/SearchPanel";
import {
  filterTasks,
  getBoard,
  getHealth,
  getWorkspaces,
  login,
  moveTask,
  searchTasks
} from "../services/api/client";
import { connectUpdatesHub } from "../services/realtime/updatesClient";
import type { TaskItem, Workspace } from "../types/models";

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

  const handleMoveTask = async (task: TaskItem) => {
    const next = task.status === "todo" ? "in-progress" : task.status === "in-progress" ? "done" : "todo";
    const updated = await moveTask(task.id, next);

    setTasks((current) => current.map((item) => (item.id === updated.id ? updated : item)));
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

      <KanbanBoard tasks={visibleTasks} onMoveTask={handleMoveTask} />

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

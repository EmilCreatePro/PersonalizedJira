import type { Workspace } from "../../../types/models";

type Props = {
  workspaces: Workspace[];
  selectedWorkspaceId: string | null;
  onSelectWorkspace: (workspace: Workspace) => void;
};

export function DashboardPage({ workspaces, selectedWorkspaceId, onSelectWorkspace }: Props) {
  return (
    <section className="panel">
      <h2>Dashboard</h2>
      <p className="muted">Select a workspace to load its board.</p>

      <div className="workspace-grid">
        {workspaces.map((workspace) => (
          <button
            key={workspace.id}
            type="button"
            className={`workspace-card ${selectedWorkspaceId === workspace.id ? "selected" : ""}`}
            onClick={() => onSelectWorkspace(workspace)}
          >
            <span>{workspace.name}</span>
            <small>{workspace.id.slice(0, 8)}</small>
          </button>
        ))}
      </div>
    </section>
  );
}

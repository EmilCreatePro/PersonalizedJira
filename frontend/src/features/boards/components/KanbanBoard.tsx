import { useState } from "react";
import type { TaskItem } from "../../../types/models";
import { TaskCard } from "../../tasks/components/TaskCard";

type Props = {
  tasks: TaskItem[];
  onMoveTask: (task: TaskItem, targetStatus?: string) => void;
};

const COLUMNS = ["todo", "in-progress", "done"];

export function KanbanBoard({ tasks, onMoveTask }: Props) {
  const [draggingTaskId, setDraggingTaskId] = useState<string | null>(null);
  const [dropTargetStatus, setDropTargetStatus] = useState<string | null>(null);

  const handleDrop = (status: string) => {
    if (!draggingTaskId) {
      setDropTargetStatus(null);
      return;
    }

    const task = tasks.find((item) => item.id === draggingTaskId);
    if (task && task.status !== status) {
      onMoveTask(task, status);
    }

    setDraggingTaskId(null);
    setDropTargetStatus(null);
  };

  return (
    <section className="panel">
      <h2>Kanban Board View</h2>
      <div className="kanban-grid">
        {COLUMNS.map((status) => (
          <div
            key={status}
            className={`kanban-column ${dropTargetStatus === status ? "drop-target" : ""}`}
            onDragOver={(event) => {
              event.preventDefault();
              setDropTargetStatus(status);
            }}
            onDragEnter={(event) => {
              event.preventDefault();
              setDropTargetStatus(status);
            }}
            onDragLeave={() => {
              setDropTargetStatus((current) => (current === status ? null : current));
            }}
            onDrop={(event) => {
              event.preventDefault();
              handleDrop(status);
            }}
          >
            <header>
              <h3>{status.toUpperCase()}</h3>
              <span>{tasks.filter((task) => task.status === status).length}</span>
            </header>

            <div className="column-stack">
              {tasks
                .filter((task) => task.status === status)
                .map((task) => (
                  <TaskCard
                    key={task.id}
                    task={task}
                    onMoveTask={onMoveTask}
                    onDragStart={(dragged) => setDraggingTaskId(dragged.id)}
                    onDragEnd={() => {
                      setDraggingTaskId(null);
                      setDropTargetStatus(null);
                    }}
                  />
                ))}
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}

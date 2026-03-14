import type { TaskItem } from "../../../types/models";
import { TaskCard } from "../../tasks/components/TaskCard";

type Props = {
  tasks: TaskItem[];
  onMoveTask: (task: TaskItem) => void;
};

const COLUMNS = ["todo", "in-progress", "done"];

export function KanbanBoard({ tasks, onMoveTask }: Props) {
  return (
    <section className="panel">
      <h2>Kanban Board View</h2>
      <div className="kanban-grid">
        {COLUMNS.map((status) => (
          <div key={status} className="kanban-column">
            <header>
              <h3>{status.toUpperCase()}</h3>
              <span>{tasks.filter((task) => task.status === status).length}</span>
            </header>

            <div className="column-stack">
              {tasks
                .filter((task) => task.status === status)
                .map((task) => (
                  <TaskCard key={task.id} task={task} onMoveTask={onMoveTask} />
                ))}
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}

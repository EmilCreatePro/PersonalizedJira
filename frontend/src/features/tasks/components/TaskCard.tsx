import type { TaskItem } from "../../../types/models";

type Props = {
  task: TaskItem;
  onMoveTask: (task: TaskItem, targetStatus?: string) => void;
  onDragStart?: (task: TaskItem) => void;
  onDragEnd?: () => void;
};

const STATUS_ORDER = ["todo", "in-progress", "done"];

export function TaskCard({ task, onMoveTask, onDragStart, onDragEnd }: Props) {
  const nextStatus = STATUS_ORDER[(STATUS_ORDER.indexOf(task.status) + 1) % STATUS_ORDER.length] ?? "todo";

  return (
    <article
      className="task-card"
      draggable
      onDragStart={() => onDragStart?.(task)}
      onDragEnd={() => onDragEnd?.()}
    >
      <h4>{task.title}</h4>
      <p>{task.description}</p>

      <div className="meta-row">
        <span>@{task.assignee}</span>
        <span>Due {task.dueDate}</span>
        <span className="tag">{task.label}</span>
      </div>

      <button type="button" onClick={() => onMoveTask(task)}>
        Move to {nextStatus}
      </button>
    </article>
  );
}

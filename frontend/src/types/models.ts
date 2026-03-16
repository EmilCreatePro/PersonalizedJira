export type AuthResponse = {
  token: string;
  displayName: string;
};

export type Workspace = {
  id: string;
  name: string;
};

export type TaskItem = {
  id: string;
  workspaceId: string;
  title: string;
  description: string;
  assignee: string;
  dueDate: string;
  label: string;
  status: "todo" | "in-progress" | "done" | string;
};

export type SearchResponse = {
  query: string;
  tasks: TaskItem[];
};

export type CreateTaskInput = {
  title: string;
  description: string;
  assignee: string;
  dueDate: string;
  label: string;
  status: "todo" | "in-progress" | "done";
};

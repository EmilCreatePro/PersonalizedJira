import type { AuthResponse, CreateTaskInput, SearchResponse, TaskItem, Workspace } from "../../types/models";

const API_BASE_URL = "http://localhost:5000";

async function apiRequest<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {})
    },
    ...init
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed: ${response.status}`);
  }

  return (await response.json()) as T;
}

export function getHealth() {
  return apiRequest<{ status: string; service: string }>("/api/health");
}

export function login(email: string, password: string) {
  return apiRequest<AuthResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password })
  });
}

export function getWorkspaces() {
  return apiRequest<Workspace[]>("/api/workspaces");
}

export function getBoard(workspaceId: string) {
  return apiRequest<TaskItem[]>(`/api/boards/${workspaceId}`);
}

export function searchTasks(query: string) {
  const encoded = encodeURIComponent(query);
  return apiRequest<SearchResponse>(`/api/search?q=${encoded}`);
}

export function filterTasks(assignee?: string, label?: string) {
  const params = new URLSearchParams();
  if (assignee) params.set("assignee", assignee);
  if (label) params.set("label", label);
  return apiRequest<TaskItem[]>(`/api/tasks/filter?${params.toString()}`);
}

export function moveTask(taskId: string, status: string) {
  return apiRequest<TaskItem>(`/api/tasks/${taskId}/move`, {
    method: "POST",
    body: JSON.stringify({ status })
  });
}

export function createTask(workspaceId: string, input: CreateTaskInput) {
  return apiRequest<TaskItem>(`/api/boards/${workspaceId}/tasks`, {
    method: "POST",
    body: JSON.stringify(input)
  });
}

export async function deleteTask(taskId: string) {
  await apiRequest<void>(`/api/tasks/${taskId}`, {
    method: "DELETE"
  });
}

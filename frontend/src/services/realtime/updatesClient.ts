import * as signalR from "@microsoft/signalr";
import type { TaskItem } from "../../types/models";

const HUB_URL = "http://localhost:5000/hubs/updates";

export type TaskMovedEvent = {
  id: string;
  status: string;
  message: string;
};

export type TaskCreatedEvent = {
  task: TaskItem;
  message: string;
};

export type TaskDeletedEvent = {
  id: string;
  message: string;
};

type RealtimeHandlers = {
  onTaskMoved: (payload: TaskMovedEvent) => void;
  onTaskCreated: (payload: TaskCreatedEvent) => void;
  onTaskDeleted: (payload: TaskDeletedEvent) => void;
  onPong: (message: string) => void;
};

export async function connectUpdatesHub(handlers: RealtimeHandlers) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .build();

  connection.on("taskMoved", handlers.onTaskMoved);
  connection.on("taskCreated", handlers.onTaskCreated);
  connection.on("taskDeleted", handlers.onTaskDeleted);
  connection.on("pong", handlers.onPong);

  await connection.start();

  return {
    connection,
    ping: async () => {
      await connection.invoke("Ping");
    },
    stop: async () => {
      await connection.stop();
    }
  };
}

import * as signalR from "@microsoft/signalr";

const HUB_URL = "http://localhost:5000/hubs/updates";

export type TaskMovedEvent = {
  id: string;
  status: string;
  message: string;
};

type RealtimeHandlers = {
  onTaskMoved: (payload: TaskMovedEvent) => void;
  onPong: (message: string) => void;
};

export async function connectUpdatesHub(handlers: RealtimeHandlers) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .build();

  connection.on("taskMoved", handlers.onTaskMoved);
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

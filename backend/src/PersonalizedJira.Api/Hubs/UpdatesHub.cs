using Microsoft.AspNetCore.SignalR;

namespace PersonalizedJira.Api.Hubs;

public sealed class UpdatesHub : Hub
{
    public Task Ping() => Clients.Caller.SendAsync("pong", "Backend realtime channel is working.");
}

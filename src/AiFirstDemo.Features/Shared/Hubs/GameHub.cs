using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Features.Shared.Hubs;

public class GameHub : Hub
{
    private readonly ILogger<GameHub> _logger;

    public GameHub(ILogger<GameHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinGameRoom(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"game_{gameId}");
        _logger.LogInformation("User {ConnectionId} joined game {GameId}", Context.ConnectionId, gameId);
    }

    public async Task LeaveGameRoom(string gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game_{gameId}");
        _logger.LogInformation("User {ConnectionId} left game {GameId}", Context.ConnectionId, gameId);
    }

    public async Task SendScoreUpdate(string gameId, int score, string playerName)
    {
        await Clients.Group($"game_{gameId}").SendAsync("ScoreUpdate", new { score, playerName, timestamp = DateTime.UtcNow });
    }

    public async Task SendGameEvent(string gameId, string eventType, object data)
    {
        await Clients.Group($"game_{gameId}").SendAsync("GameEvent", new { eventType, data, timestamp = DateTime.UtcNow });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Features.Shared.Hubs;

public class AnalyticsHub : Hub
{
    private readonly ILogger<AnalyticsHub> _logger;

    public AnalyticsHub(ILogger<AnalyticsHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinAnalyticsRoom()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "analytics");
        _logger.LogInformation("Analytics viewer {ConnectionId} connected", Context.ConnectionId);
    }

    public async Task LeaveAnalyticsRoom()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "analytics");
    }

    public async Task SendAnalyticsUpdate(object data)
    {
        await Clients.Group("analytics").SendAsync("AnalyticsUpdate", new { data, timestamp = DateTime.UtcNow });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Analytics viewer {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
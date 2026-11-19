using HotelOS.Api.Hubs;
using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Reporting;
using Microsoft.AspNetCore.SignalR;

namespace HotelOS.Api.Services;

public class RealtimeNotificationSender : IRealtimeNotificationSender
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public RealtimeNotificationSender(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default)
    {
        if (notification.UserId.HasValue)
        {
            await _hubContext.Clients.User(notification.UserId.Value.ToString())
                .SendAsync("ReceiveNotification", notification, cancellationToken);
        }
        else
        {
            await _hubContext.Clients.Group($"Property_{notification.PropertyId}")
                .SendAsync("ReceiveNotification", notification, cancellationToken);
            await _hubContext.Clients.Group("AllUsers")
                .SendAsync("ReceiveNotification", notification, cancellationToken);
        }
    }

    public async Task BroadcastLiveEventAsync(string eventType, object payload, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveLiveEvent", eventType, payload, cancellationToken);
    }
}

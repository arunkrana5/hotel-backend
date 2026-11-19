using HotelOS.Contracts.Reporting;

namespace HotelOS.Application.Common.Interfaces;

public interface IRealtimeNotificationSender
{
    Task BroadcastNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default);
    Task BroadcastLiveEventAsync(string eventType, object payload, CancellationToken cancellationToken = default);
}

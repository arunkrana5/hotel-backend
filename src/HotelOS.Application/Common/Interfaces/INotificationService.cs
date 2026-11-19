using HotelOS.Contracts.Reporting;

namespace HotelOS.Application.Common.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetNotificationsAsync(
        long? userId,
        long propertyId = 1,
        int limit = 20,
        CancellationToken cancellationToken = default);

    Task<NotificationDto> CreateNotificationAsync(
        string title,
        string message,
        string type = "INFO",
        string? targetUrl = null,
        long? userId = null,
        long propertyId = 1,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(
        long notificationId,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(
        long? userId,
        long propertyId = 1,
        CancellationToken cancellationToken = default);
}

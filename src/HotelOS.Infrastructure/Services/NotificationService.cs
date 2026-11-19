using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Reporting;
using HotelOS.Domain.Identity.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly HotelOSDbContext _context;
    private readonly IRealtimeNotificationSender? _realtimeSender;

    public NotificationService(
        HotelOSDbContext context,
        IRealtimeNotificationSender? realtimeSender = null)
    {
        _context = context;
        _realtimeSender = realtimeSender;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(
        long? userId,
        long propertyId = 1,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.PropertyId == propertyId && !n.IsDeleted);

        if (userId.HasValue)
        {
            query = query.Where(n => n.UserId == null || n.UserId == userId.Value);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .Select(n => new NotificationDto(
                n.Id,
                n.PropertyId,
                n.UserId,
                n.Title,
                n.Message,
                n.Type,
                n.TargetUrl,
                n.IsRead,
                n.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationDto> CreateNotificationAsync(
        string title,
        string message,
        string type = "INFO",
        string? targetUrl = null,
        long? userId = null,
        long propertyId = 1,
        CancellationToken cancellationToken = default)
    {
        var notification = new AppNotification
        {
            PropertyId = propertyId,
            UserId = userId,
            Title = title.Trim(),
            Message = message.Trim(),
            Type = type.Trim().ToUpperInvariant(),
            TargetUrl = targetUrl?.Trim(),
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "SYSTEM"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new NotificationDto(
            notification.Id,
            notification.PropertyId,
            notification.UserId,
            notification.Title,
            notification.Message,
            notification.Type,
            notification.TargetUrl,
            notification.IsRead,
            notification.CreatedAt
        );

        if (_realtimeSender != null)
        {
            try
            {
                await _realtimeSender.BroadcastNotificationAsync(dto, cancellationToken);
            }
            catch
            {
                // Non-blocking if real-time sender is unavailable
            }
        }

        return dto;
    }

    public async Task<bool> MarkAsReadAsync(
        long notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsDeleted, cancellationToken);

        if (notification == null) return false;

        notification.IsRead = true;
        notification.ReadAt = DateTimeOffset.UtcNow;
        notification.ModifiedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(
        long? userId,
        long propertyId = 1,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .Where(n => n.PropertyId == propertyId && !n.IsRead && !n.IsDeleted);

        if (userId.HasValue)
        {
            query = query.Where(n => n.UserId == null || n.UserId == userId.Value);
        }

        var unread = await query.ToListAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
            n.ModifiedAt = now;
        }

        return await _context.SaveChangesAsync(cancellationToken);
    }
}

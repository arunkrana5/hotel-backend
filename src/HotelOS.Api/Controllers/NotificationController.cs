using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationController(INotificationService notificationService, ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetNotifications([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        var propertyId = _currentUserService.PropertyId ?? 1;
        var notifications = await _notificationService.GetNotificationsAsync(userId, propertyId, limit, ct);
        return Ok(ApiResponse<List<NotificationDto>>.Ok(notifications));
    }

    [HttpPut("{id:long}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(long id, CancellationToken ct = default)
    {
        var success = await _notificationService.MarkAsReadAsync(id, ct);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Notification not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Notification marked as read"));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse<int>>> MarkAllAsRead(CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        var propertyId = _currentUserService.PropertyId ?? 1;
        var count = await _notificationService.MarkAllAsReadAsync(userId, propertyId, ct);
        return Ok(ApiResponse<int>.Ok(count, $"Marked {count} notifications as read"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> CreateNotification([FromBody] CreateNotificationRequest request, CancellationToken ct = default)
    {
        var targetPropertyId = _currentUserService.PropertyId ?? 1;
        var notification = await _notificationService.CreateNotificationAsync(
            request.Title,
            request.Message,
            request.Type ?? "INFO",
            request.TargetUrl,
            request.UserId,
            targetPropertyId,
            ct);

        return Ok(ApiResponse<NotificationDto>.Ok(notification, "Notification broadcast successfully"));
    }
}

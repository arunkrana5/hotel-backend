namespace HotelOS.Contracts.Reporting;

public record NotificationDto(
    long Id,
    long PropertyId,
    long? UserId,
    string Title,
    string Message,
    string Type,
    string? TargetUrl,
    bool IsRead,
    DateTimeOffset CreatedAt
);

public record CreateNotificationRequest(
    long? UserId,
    string Title,
    string Message,
    string Type,
    string? TargetUrl
);

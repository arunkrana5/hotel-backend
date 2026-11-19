namespace HotelOS.Domain.Identity.Entities;

using HotelOS.Domain.Common;

public class AppNotification : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public long? UserId { get; set; }
    public User? User { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "INFO"; // INFO, SUCCESS, WARNING, URGENT
    public string? TargetUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTimeOffset? ReadAt { get; set; }
}

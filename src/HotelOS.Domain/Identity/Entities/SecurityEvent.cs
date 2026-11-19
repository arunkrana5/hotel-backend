using HotelOS.Domain.Identity.Enums;

namespace HotelOS.Domain.Identity.Entities;

public class SecurityEvent
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public SecurityEventType EventType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
}

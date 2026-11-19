namespace HotelOS.Domain.Identity.Entities;

public class LoginHistory
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public Guid? SessionId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTimeOffset LoginAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LogoutAt { get; set; }

    public User? User { get; set; }
}

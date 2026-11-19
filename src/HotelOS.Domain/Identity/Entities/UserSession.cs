namespace HotelOS.Domain.Identity.Entities;

public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long UserId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public Guid? ReplacedBySessionId { get; set; }

    public User? User { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

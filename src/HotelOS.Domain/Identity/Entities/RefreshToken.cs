namespace HotelOS.Domain.Identity.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public Guid SessionId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public User? User { get; set; }
    public UserSession? Session { get; set; }
}

namespace HotelOS.Domain.Identity.Entities;

public class PasswordResetToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTimeOffset? UsedAt { get; set; }

    public User? User { get; set; }
}

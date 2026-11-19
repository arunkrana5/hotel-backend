namespace HotelOS.Domain.Identity.Entities;

public class PasswordHistory
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
}

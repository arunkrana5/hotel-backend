namespace HotelOS.Domain.Identity.Entities;

public class UserCredential
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset PasswordUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
}

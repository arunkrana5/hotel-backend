namespace HotelOS.Domain.Identity.Entities;

public class UserRole
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? AssignedBy { get; set; }

    public User? User { get; set; }
    public Role? Role { get; set; }
}

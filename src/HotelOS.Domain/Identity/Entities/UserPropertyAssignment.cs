namespace HotelOS.Domain.Identity.Entities;

public class UserPropertyAssignment
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long PropertyId { get; set; }
    public bool IsDefault { get; set; } = false;
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? AssignedBy { get; set; }

    public User? User { get; set; }
}

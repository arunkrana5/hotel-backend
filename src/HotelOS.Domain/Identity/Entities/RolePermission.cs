namespace HotelOS.Domain.Identity.Entities;

public class RolePermission
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? GrantedBy { get; set; }

    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}

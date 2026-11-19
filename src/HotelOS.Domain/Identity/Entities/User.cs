using HotelOS.Domain.Common;
using HotelOS.Domain.Identity.Enums;

namespace HotelOS.Domain.Identity.Entities;

public class User : BaseEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string NormalizedUsername { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? EmployeeId { get; set; }
    public long? DepartmentId { get; set; }
    public long? DesignationId { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public bool EmailConfirmed { get; set; } = false;
    public bool PhoneNumberConfirmed { get; set; } = false;
    public bool TwoFactorEnabled { get; set; } = false;
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; } = 0;
    public bool MustChangePassword { get; set; } = false;
    public DateTimeOffset? LastLoginAt { get; set; }

    // Navigation properties
    public UserCredential? Credential { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public ICollection<UserPropertyAssignment> PropertyAssignments { get; set; } = new List<UserPropertyAssignment>();
    public ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();
    public ICollection<SecurityEvent> SecurityEvents { get; set; } = new List<SecurityEvent>();
    public ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
}

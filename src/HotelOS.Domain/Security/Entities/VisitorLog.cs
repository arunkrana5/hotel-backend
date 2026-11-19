namespace HotelOS.Domain.Security.Entities;

using HotelOS.Domain.Common;
using HotelOS.Domain.Identity.Entities;

public class VisitorLog : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string PassNumber { get; set; } = string.Empty;
    public string VisitorName { get; set; } = string.Empty;
    public string IdType { get; set; } = "NATIONAL_ID"; // NATIONAL_ID, PASSPORT, DRIVERS_LICENSE
    public string IdNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string VisitingDepartment { get; set; } = "FO";
    public string? RoomOrArea { get; set; }
    public DateTimeOffset CheckInTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CheckOutTime { get; set; }
    public string Status { get; set; } = "IN_PREMISES"; // IN_PREMISES, CHECKED_OUT
    public string? BadgeNumber { get; set; }
}

public class IncidentReport : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string IncidentNumber { get; set; } = string.Empty;
    public DateTimeOffset IncidentDate { get; set; } = DateTimeOffset.UtcNow;
    public string Location { get; set; } = string.Empty;
    public string Severity { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, CRITICAL
    public string Category { get; set; } = "SECURITY"; // THEFT, PROPERTY_DAMAGE, GUEST_DISPUTE, MEDICAL, FIRE_SAFETY, TRESPASS
    public string Description { get; set; } = string.Empty;
    public long? ReportedByUserId { get; set; }
    public User? ReportedByUser { get; set; }
    public string Status { get; set; } = "REPORTED"; // REPORTED, INVESTIGATING, RESOLVED, CLOSED
    public string? ActionTaken { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}

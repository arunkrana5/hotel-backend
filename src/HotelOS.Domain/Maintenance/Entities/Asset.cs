namespace HotelOS.Domain.Maintenance.Entities;

using HotelOS.Domain.Common;
using HotelOS.Domain.Identity.Entities;
using HotelOS.Domain.Pms.Entities;

public class Asset : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string Category { get; set; } = "HVAC"; // HVAC, ELECTRICAL, PLUMBING, KITCHEN_EQUIPMENT, ELEVATOR, IT_HARDWARE
    public string Location { get; set; } = string.Empty;
    public DateOnly PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public DateOnly? WarrantyExpiryDate { get; set; }
    public string Status { get; set; } = "OPERATIONAL"; // OPERATIONAL, UNDER_MAINTENANCE, DECOMMISSIONED
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
}

public class WorkOrder : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string WorkOrderNumber { get; set; } = string.Empty;
    public long? AssetId { get; set; }
    public Asset? Asset { get; set; }
    public long? RoomId { get; set; }
    public Room? Room { get; set; }
    public string DepartmentCode { get; set; } = "ENG";
    public string IssueDescription { get; set; } = string.Empty;
    public string Priority { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, EMERGENCY
    public string Status { get; set; } = "OPEN"; // OPEN, ASSIGNED, IN_PROGRESS, COMPLETED, CANCELLED
    public long? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public decimal TotalCost { get; set; } = 0;
}

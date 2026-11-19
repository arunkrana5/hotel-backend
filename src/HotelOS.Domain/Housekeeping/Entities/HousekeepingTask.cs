namespace HotelOS.Domain.Housekeeping.Entities;

using HotelOS.Domain.Common;
using HotelOS.Domain.Identity.Entities;
using HotelOS.Domain.Pms.Entities;

public class HousekeepingTask : BaseEntity
{
    public long PropertyId { get; set; }
    public string TaskNumber { get; set; } = string.Empty;
    public long RoomId { get; set; }
    public Room? Room { get; set; }
    public string TaskType { get; set; } = "DAILY_CLEAN";
    public string Priority { get; set; } = "MEDIUM";
    public string Status { get; set; } = "PENDING";
    public long? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public long? InspectedByUserId { get; set; }
    public User? InspectedByUser { get; set; }
    public DateTimeOffset? InspectedAt { get; set; }
    public string? Remarks { get; set; }
    public string? ChecklistJson { get; set; }
}

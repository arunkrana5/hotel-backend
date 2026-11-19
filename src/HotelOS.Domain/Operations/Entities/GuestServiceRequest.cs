namespace HotelOS.Domain.Operations.Entities;

using HotelOS.Domain.Common;
using HotelOS.Domain.Identity.Entities;
using HotelOS.Domain.Pms.Entities;

public class GuestServiceRequest : BaseEntity
{
    public long PropertyId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public long RoomId { get; set; }
    public Room? Room { get; set; }
    public long? GuestId { get; set; }
    public Guest? Guest { get; set; }
    public string RequestType { get; set; } = "EXTRA_TOWELS"; // EXTRA_TOWELS, EXTRA_BED, ROOM_CLEANING, MAINTENANCE_ISSUE, BELLBOY_LUGGAGE, WAKEUP_CALL, CONCIERGE
    public string Department { get; set; } = "HOUSEKEEPING"; // HOUSEKEEPING, FRONT_OFFICE, MAINTENANCE, CONCIERGE, RESTAURANT
    public string Priority { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, URGENT
    public string Status { get; set; } = "PENDING"; // PENDING, ASSIGNED, IN_PROGRESS, RESOLVED, CANCELLED
    public string Description { get; set; } = string.Empty;
    public long? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}

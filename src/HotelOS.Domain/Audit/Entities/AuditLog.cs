namespace HotelOS.Domain.Audit.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Transition, MasterChange
    public string? ChangesJson { get; set; }
    public string PerformedBy { get; set; } = "SYSTEM";
    public DateTimeOffset PerformedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? IpAddress { get; set; }
    public long? PropertyId { get; set; }
}

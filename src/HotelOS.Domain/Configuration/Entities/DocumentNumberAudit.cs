namespace HotelOS.Domain.Configuration.Entities;

public class DocumentNumberAudit
{
    public long Id { get; set; }
    public long PropertyId { get; set; } = 1;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string GeneratedBy { get; set; } = "SYSTEM";
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
    public long? EntityId { get; set; }
    public string? EntityType { get; set; }
    public string Status { get; set; } = "COMMITTED"; // "COMMITTED", "RESERVED", "CANCELLED"
}

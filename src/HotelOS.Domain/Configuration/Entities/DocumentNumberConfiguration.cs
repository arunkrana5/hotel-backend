namespace HotelOS.Domain.Configuration.Entities;

public class DocumentNumberConfiguration
{
    public long Id { get; set; }
    public long PropertyId { get; set; } = 1;
    public string DocumentType { get; set; } = string.Empty; // e.g. "RESERVATION", "JOURNAL_VOUCHER"
    public string Module { get; set; } = string.Empty;       // e.g. "PMS", "FINANCE", "PURCHASE"
    public string Prefix { get; set; } = string.Empty;       // e.g. "RES", "JV", "PO"
    public string? Suffix { get; set; }
    public string DateFormat { get; set; } = "yyyyMMdd";    // e.g. "yyyyMMdd", "yyyyMM", "NONE"
    public int SequenceLength { get; set; } = 4;            // e.g. 4 -> 0001
    public long StartingNumber { get; set; } = 1;
    public string ResetFrequency { get; set; } = "DAILY";    // "DAILY", "MONTHLY", "YEARLY", "NEVER"
    public long CurrentSequence { get; set; } = 0;
    public DateOnly LastResetDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; set; } = "SYSTEM";
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public int Version { get; set; } = 1;
}

namespace HotelOS.Contracts.Configuration;

public record DocumentNumberConfigurationDto(
    long Id,
    long PropertyId,
    string DocumentType,
    string Module,
    string Prefix,
    string? Suffix,
    string DateFormat,
    int SequenceLength,
    long StartingNumber,
    string ResetFrequency,
    long CurrentSequence,
    string LastResetDate,
    bool IsActive,
    string SamplePreview
);

public record UpdateDocumentNumberConfigRequest(
    string Prefix,
    string? Suffix,
    string DateFormat,
    int SequenceLength,
    long StartingNumber,
    string ResetFrequency,
    bool IsActive
);

public record NextDocumentNumberResponse(
    string DocumentType,
    string DocumentNumber,
    long SequenceNumber,
    DateTimeOffset Timestamp
);

public record DocumentNumberAuditDto(
    long Id,
    long PropertyId,
    string DocumentType,
    string DocumentNumber,
    string GeneratedBy,
    DateTimeOffset GeneratedAt,
    long? EntityId,
    string? EntityType,
    string Status
);

namespace HotelOS.Contracts.Banquet;

public record BanquetVenueDto(
    long Id,
    string VenueCode,
    string VenueName,
    string Location,
    int MaxCapacity,
    decimal HourlyRate,
    decimal DayRate,
    string Status,
    string? AmenitiesJson
);

public record BanquetBookingDto(
    long Id,
    string BookingNumber,
    long VenueId,
    string VenueName,
    string EventName,
    string EventType,
    string ClientName,
    string ClientPhone,
    string ClientEmail,
    long? GuestId,
    string? GuestName,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    int ExpectedGuests,
    string Status,
    decimal TotalAmount,
    decimal AdvancePaid,
    decimal BalanceAmount,
    string? CateringPackage,
    string? SetupStyle,
    string? SpecialRequests,
    DateTimeOffset CreatedAt
);

public record CreateBanquetBookingRequest(
    long VenueId,
    string EventName,
    string EventType,
    string ClientName,
    string ClientPhone,
    string ClientEmail,
    long? GuestId,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    int ExpectedGuests,
    decimal TotalAmount,
    decimal AdvancePaid,
    string? CateringPackage,
    string? SetupStyle,
    string? SpecialRequests
);

public record UpdateBanquetBookingStatusRequest(
    string Status
);

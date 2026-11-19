namespace HotelOS.Contracts.Operations;

public record GuestServiceRequestDto(
    long Id,
    string RequestNumber,
    long RoomId,
    string RoomNumber,
    int FloorNumber,
    long? GuestId,
    string? GuestName,
    string RequestType,
    string Department,
    string Priority,
    string Status,
    string Description,
    long? AssignedToUserId,
    string? AssignedToUserName,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ResolvedAt,
    string? ResolutionNotes
);

public record CreateGuestServiceRequest(
    long RoomId,
    long? GuestId,
    string RequestType,
    string Department,
    string Priority,
    string Description,
    long? AssignedToUserId
);

public record UpdateGuestServiceRequestStatus(
    string Status,
    string? ResolutionNotes,
    long? AssignedToUserId
);

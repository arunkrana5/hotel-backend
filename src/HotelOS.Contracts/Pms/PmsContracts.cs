namespace HotelOS.Contracts.Pms;

public record RoomDto(
    long Id,
    string RoomNumber,
    int FloorNumber,
    string RoomTypeCode,
    string RoomStatusCode,
    int MaxOccupancy,
    decimal BaseRatePerNight,
    bool IsSmoking,
    string? KeyCardUid,
    string? Notes
);

public record FloorRoomsDto(
    int FloorNumber,
    List<RoomDto> Rooms
);

public record RoomGridSummaryDto(
    int TotalRooms,
    int OccupiedCount,
    int CleanCount,
    int DirtyCount,
    int InspectedCount,
    int MaintenanceCount,
    int OutOfOrderCount,
    List<FloorRoomsDto> Floors
);

public record GuestDto(
    long Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Phone,
    string? IdDocumentType,
    string? IdDocumentNumber,
    string? Nationality,
    string VipStatus,
    int TotalStays,
    decimal LifetimeSpend,
    string? SpecialPreferences,
    string? City,
    string? Country,
    bool IsActive = true,
    string? Address = null
);

public record CreateGuestRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? IdDocumentType = null,
    string? IdDocumentNumber = null,
    string? Nationality = null,
    string VipStatus = "Standard",
    string? SpecialPreferences = null,
    string? Address = null,
    string? City = null,
    string? Country = null
);

public record UpdateGuestRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? IdDocumentType = null,
    string? IdDocumentNumber = null,
    string? Nationality = null,
    string VipStatus = "Standard",
    string? SpecialPreferences = null,
    string? Address = null,
    string? City = null,
    string? Country = null,
    bool IsActive = true
);

public record SetGuestStatusRequest(bool IsActive);

public record GuestCrmProfileDto(
    long Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Phone,
    string? IdDocumentType,
    string? IdDocumentNumber,
    string? Nationality,
    string VipStatus,
    string? SpecialPreferences,
    string? Address,
    string? City,
    string? Country,
    bool IsActive,
    DateTimeOffset CreatedAt,
    GuestCrmSummaryDto Summary,
    List<ReservationDto> Reservations,
    List<GuestStayHistoryDto> StayHistory,
    List<GuestFolioSummaryDto> Folios,
    List<GuestAuditEventDto> AuditLog
);

public record GuestCrmSummaryDto(
    int TotalStays,
    int UpcomingReservations,
    int CompletedStays,
    decimal TotalRevenue,
    decimal OutstandingBalance,
    string? LastStayDate,
    double AverageStayDurationNights
);

public record GuestStayHistoryDto(
    long ReservationId,
    string ConfirmationNumber,
    string RoomTypeCode,
    string? RoomNumber,
    string CheckInDate,
    string CheckOutDate,
    int Nights,
    decimal TotalAmount,
    string Status
);

public record GuestFolioSummaryDto(
    long FolioId,
    string FolioNumber,
    string ReservationConfirmation,
    string Status,
    decimal TotalCharges,
    decimal TotalPayments,
    decimal Balance,
    DateTimeOffset CreatedAt,
    List<GuestFolioItemDto> Items
);

public record GuestFolioItemDto(
    long Id,
    string ItemType,
    string DepartmentCode,
    string Description,
    decimal Amount,
    string? PaymentModeCode,
    string? TransactionReference,
    DateTimeOffset PostedAt
);

public record GuestAuditEventDto(
    string EventType,
    string Description,
    DateTimeOffset Timestamp,
    string Actor
);

public record GuestCrmStatsDto(
    int TotalCount,
    int ActiveCount,
    int VipCount,
    int InactiveCount,
    decimal TotalSpend,
    decimal AverageSpend
);

public record DuplicateCheckResult(
    bool IsDuplicate,
    GuestDto? MatchedGuest
);

public record ReservationDto(
    long Id,
    string ConfirmationNumber,
    long GuestId,
    string GuestName,
    string GuestEmail,
    string GuestPhone,
    string? GuestVip,
    string RoomTypeCode,
    long? AssignedRoomId,
    string? AssignedRoomNumber,
    string CheckInDate,
    string CheckOutDate,
    int AdultsCount,
    int ChildrenCount,
    string ReservationStatus,
    string BookingSource,
    decimal RatePerNight,
    decimal TotalEstimatedAmount,
    decimal PaidAmount,
    decimal BalanceAmount,
    string? SpecialRequests,
    DateTimeOffset? ActualCheckInAt,
    DateTimeOffset? ActualCheckOutAt
);

public record CreateReservationRequest(
    long? GuestId,
    CreateGuestRequest? NewGuest,
    string RoomTypeCode,
    string CheckInDate, // YYYY-MM-DD
    string CheckOutDate, // YYYY-MM-DD
    int AdultsCount = 1,
    int ChildrenCount = 0,
    string BookingSource = "DIRECT",
    decimal? CustomRate = null,
    string? SpecialRequests = null
);

public record CheckInRequest(
    long ReservationId,
    long RoomId,
    string? KeyCardUid = null,
    decimal? AdvanceDeposit = null,
    string? PaymentModeCode = null
);

public record CheckOutRequest(
    long ReservationId,
    decimal? SettlementPayment = null,
    string? PaymentModeCode = null,
    string? Notes = null
);

public record FolioItemDto(
    long Id,
    string ItemType,
    string DepartmentCode,
    string Description,
    decimal UnitPrice,
    int Quantity,
    decimal Amount,
    string? PaymentModeCode,
    string? TransactionReference,
    DateTimeOffset PostedAt,
    string PostedBy
);

public record FolioDto(
    long Id,
    string FolioNumber,
    long ReservationId,
    string ConfirmationNumber,
    long GuestId,
    string GuestName,
    string Status,
    decimal TotalCharges,
    decimal TotalPayments,
    decimal BalanceAmount,
    List<FolioItemDto> Items
);

public record PostChargeRequest(
    long FolioId,
    string DepartmentCode,
    string Description,
    decimal UnitPrice,
    int Quantity = 1
);

public record PostPaymentRequest(
    long FolioId,
    decimal Amount,
    string PaymentModeCode,
    string? TransactionReference = null
);

public record UpdateRoomStatusRequest(
    string StatusCode,
    string? Notes = null
);

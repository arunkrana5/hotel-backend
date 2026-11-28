namespace HotelOS.Contracts.Restaurant;

public record RoomQrCodeDto(
    long Id,
    long PropertyId,
    long RoomId,
    string RoomNumber,
    string RoomType,
    string QrToken,
    string QrUrl,
    bool RequirePin,
    string? PinCode,
    bool IsActive,
    int ScanCount,
    DateTimeOffset GeneratedAt,
    DateTimeOffset? LastScannedAt
);

public record GenerateRoomQrRequest(
    long RoomId,
    bool RequirePin = false,
    string? PinCode = null
);

public record BulkGenerateRoomQrRequest(
    List<long>? RoomIds = null,
    bool OverwriteExisting = false
);

public record QrValidationResultDto(
    bool IsValid,
    string? ErrorCode,
    string? ErrorMessage,
    long? PropertyId,
    string? PropertyName,
    long? RoomId,
    string? RoomNumber,
    string? RoomType,
    bool RequirePin,
    bool IsOccupied,
    string? GuestName,
    string? SessionToken
);

public record GuestSessionInitRequest(
    string QrToken,
    string? PinCode = null,
    string? GuestName = null,
    string? GuestPhone = null
);

public record GuestSessionDto(
    string SessionToken,
    long PropertyId,
    string PropertyName,
    long RoomId,
    string RoomNumber,
    string? GuestDisplayName,
    bool IsStayVerified,
    string VerificationMode,
    DateTimeOffset ExpiresAt
);

public record GuestPlaceOrderRequest(
    string SessionToken,
    string? GuestDisplayName,
    string? DeliveryContactPhone,
    string? SpecialInstructions,
    string PaymentMethod,
    List<CreateOrderItemRequest> Items
);

public record GuestOrderSummaryDto(
    long Id,
    string OrderNumber,
    long RoomId,
    string RoomNumber,
    string? GuestDisplayName,
    string? DeliveryContactPhone,
    bool IsStayVerified,
    string Status,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    string PaymentStatus,
    string? PaymentMethod,
    string? SpecialInstructions,
    DateTimeOffset PlacedAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? PreparedAt,
    DateTimeOffset? DeliveredAt,
    int? EstimatedDeliveryMinutes,
    string? DeliveryStaffName,
    List<RestaurantOrderItemDto> Items
);

public record GuestCreateServiceRequest(
    string SessionToken,
    string RequestType,
    string? Description
);

public record StaffUpdateRoomOrderStatusRequest(
    string Status,
    int? EstimatedDeliveryMinutes = null,
    string? DeliveryStaffName = null
);

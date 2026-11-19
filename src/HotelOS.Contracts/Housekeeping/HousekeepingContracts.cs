namespace HotelOS.Contracts.Housekeeping;

public record HousekeepingTaskDto(
    long Id,
    string TaskNumber,
    long RoomId,
    string RoomNumber,
    int FloorNumber,
    string RoomTypeCode,
    string TaskType,
    string Priority,
    string Status,
    long? AssignedToUserId,
    string? AssignedToUserName,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    long? InspectedByUserId,
    string? InspectedByUserName,
    DateTimeOffset? InspectedAt,
    string? Remarks,
    DateTimeOffset CreatedAt
);

public record CreateHousekeepingTaskRequest(
    long RoomId,
    string TaskType,
    string Priority,
    long? AssignedToUserId,
    string? Remarks
);

public record UpdateHousekeepingTaskStatusRequest(
    string Status,
    string? Remarks
);

public record AssignHousekeepingTaskRequest(
    long AssignedToUserId
);

public record LinenItemDto(
    long Id,
    string ItemCode,
    string ItemName,
    string Category,
    int ParLevel,
    int CleanStock,
    int InUseStock,
    int InLaundryStock,
    int DiscardedStock,
    string? Unit
);

public record UpdateLinenStockRequest(
    int CleanStock,
    int InUseStock,
    int InLaundryStock,
    int DiscardedStock
);

public record LaundryOrderDto(
    long Id,
    string OrderNumber,
    string OrderType,
    long? RoomId,
    string? RoomNumber,
    long? GuestId,
    string? GuestName,
    long? FolioId,
    string Status,
    int TotalPieces,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    bool IsExpress,
    string? SpecialInstructions,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset CreatedAt,
    List<LaundryOrderItemDto> Items
);

public record LaundryOrderItemDto(
    long Id,
    string ServiceType,
    string GarmentName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? Remarks
);

public record CreateLaundryOrderItemRequest(
    string ServiceType,
    string GarmentName,
    int Quantity,
    decimal UnitPrice,
    string? Remarks
);

public record CreateLaundryOrderRequest(
    string OrderType,
    long? RoomId,
    long? GuestId,
    bool IsExpress,
    string? SpecialInstructions,
    List<CreateLaundryOrderItemRequest> Items
);

public record UpdateLaundryStatusRequest(
    string Status
);

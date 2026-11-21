namespace HotelOS.Contracts.Restaurant;

public record DiningTableDto(
    long Id,
    string TableNumber,
    string Section,
    int Capacity,
    string Status,
    int DisplayOrder
);

public record UpdateTableStatusRequest(
    string Status
);

public record MenuItemDto(
    long Id,
    string ItemCode,
    string ItemName,
    string Category,
    string? Description,
    decimal Price,
    int PreparationTimeMinutes,
    string DietaryType,
    bool IsAvailable,
    string? ImageUrl
);

public record CreateMenuItemRequest(
    string ItemCode,
    string ItemName,
    string Category,
    string? Description,
    decimal Price,
    int PreparationTimeMinutes,
    string DietaryType,
    bool IsAvailable,
    string? ImageUrl
);

public record UpdateMenuItemRequest(
    string ItemName,
    string Category,
    string? Description,
    decimal Price,
    int PreparationTimeMinutes,
    string DietaryType,
    bool IsAvailable,
    string? ImageUrl = null
);

public record UpdateMenuItemAvailabilityRequest(
    bool IsAvailable
);

public record RestaurantOrderItemDto(
    long Id,
    long MenuItemId,
    string ItemName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Status,
    string? Notes
);

public record RestaurantOrderDto(
    long Id,
    string OrderNumber,
    string OrderType,
    long? TableId,
    string? TableNumber,
    long? RoomId,
    string? RoomNumber,
    long? GuestId,
    string? GuestName,
    long? FolioId,
    string Status,
    decimal SubTotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    string PaymentStatus,
    string? PaymentMethod,
    long? ServerUserId,
    string? ServerUserName,
    string? SpecialInstructions,
    DateTimeOffset PlacedAt,
    DateTimeOffset? PreparedAt,
    DateTimeOffset? ServedAt,
    List<RestaurantOrderItemDto> Items
);

public record CreateOrderItemRequest(
    long MenuItemId,
    int Quantity,
    string? Notes
);

public record CreateRestaurantOrderRequest(
    string OrderType,
    long? TableId,
    long? RoomId,
    long? GuestId,
    string? SpecialInstructions,
    List<CreateOrderItemRequest> Items
);

public record UpdateOrderStatusRequest(
    string Status
);

public record UpdateOrderItemStatusRequest(
    string Status
);

public record SettleRestaurantOrderRequest(
    string PaymentStatus,
    string? PaymentMethod
);

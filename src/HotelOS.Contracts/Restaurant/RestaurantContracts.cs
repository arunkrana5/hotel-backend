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

public record MenuDto(
    long Id,
    long PropertyId,
    string MenuCode,
    string MenuName,
    string? Description,
    string? ImageUrl,
    string MenuType, // ALL_DAY, BREAKFAST, LUNCH, DINNER, LATE_NIGHT, SPECIAL
    string Status, // DRAFT, PUBLISHED, UNPUBLISHED, SCHEDULED, ARCHIVED
    bool IsGuestVisible,
    int DisplayPriority,
    string AvailableDays,
    string? StartTime,
    string? EndTime,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveUntil,
    string ApplicableOutlets,
    int ItemCount,
    int CategoryCount
);

public record CreateMenuRequest(
    string MenuCode,
    string MenuName,
    string? Description,
    string? ImageUrl,
    string MenuType,
    string Status,
    bool IsGuestVisible,
    int DisplayPriority,
    string AvailableDays,
    string? StartTime,
    string? EndTime,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveUntil,
    string? ApplicableOutlets
);

public record UpdateMenuRequest(
    string MenuName,
    string? Description,
    string? ImageUrl,
    string MenuType,
    string Status,
    bool IsGuestVisible,
    int DisplayPriority,
    string AvailableDays,
    string? StartTime,
    string? EndTime,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveUntil,
    string? ApplicableOutlets
);

public record MenuCategoryDto(
    long Id,
    long PropertyId,
    long? MenuId,
    string? MenuName,
    string CategoryCode,
    string CategoryName,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsGuestVisible,
    string ApplicableOutlets,
    int ItemCount
);

public record CreateMenuCategoryRequest(
    long? MenuId,
    string CategoryCode,
    string CategoryName,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsGuestVisible,
    string? ApplicableOutlets
);

public record UpdateMenuCategoryRequest(
    long? MenuId,
    string CategoryName,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsGuestVisible,
    string? ApplicableOutlets
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
    string? ImageUrl,
    long? MenuId = null,
    string? MenuName = null,
    long? MenuCategoryId = null,
    string? CategoryName = null,
    bool IsGuestVisible = true,
    bool IsFeatured = false,
    int DisplayOrder = 0,
    decimal TaxPercent = 5.0m,
    string Status = "PUBLISHED",
    string? Allergens = null,
    string? VariantsJson = null,
    string? AddOnsJson = null,
    string ApplicableOutlets = "ROOM_SERVICE,MAIN_RESTAURANT"
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
    string? ImageUrl,
    long? MenuId = null,
    long? MenuCategoryId = null,
    bool IsGuestVisible = true,
    bool IsFeatured = false,
    int DisplayOrder = 0,
    decimal TaxPercent = 5.0m,
    string Status = "PUBLISHED",
    string? Allergens = null,
    string? VariantsJson = null,
    string? AddOnsJson = null,
    string? ApplicableOutlets = null
);

public record UpdateMenuItemRequest(
    string ItemName,
    string Category,
    string? Description,
    decimal Price,
    int PreparationTimeMinutes,
    string DietaryType,
    bool IsAvailable,
    string? ImageUrl = null,
    long? MenuId = null,
    long? MenuCategoryId = null,
    bool IsGuestVisible = true,
    bool IsFeatured = false,
    int DisplayOrder = 0,
    decimal TaxPercent = 5.0m,
    string Status = "PUBLISHED",
    string? Allergens = null,
    string? VariantsJson = null,
    string? AddOnsJson = null,
    string? ApplicableOutlets = null
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
    string? Notes,
    string? SelectedVariantsJson = null,
    string? SelectedAddOnsJson = null,
    decimal BasePrice = 0,
    decimal OptionsPrice = 0
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
    List<RestaurantOrderItemDto> Items,
    DateTimeOffset? DeliveredAt = null,
    string? Source = null,
    string? DeliveryContactPhone = null,
    string? GuestDisplayName = null,
    bool IsStayVerified = false,
    int? EstimatedDeliveryMinutes = null,
    string? DeliveryStaffName = null
);

public record CreateOrderItemRequest(
    long MenuItemId,
    int Quantity,
    string? Notes,
    string? SelectedVariantsJson = null,
    string? SelectedAddOnsJson = null,
    decimal OptionsPrice = 0
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

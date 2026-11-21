namespace HotelOS.Contracts.Maintenance;

public record AssetDto(
    long Id,
    string AssetCode,
    string AssetName,
    string Category,
    string Location,
    string PurchaseDate,
    decimal PurchaseCost,
    string? WarrantyExpiryDate,
    string Status,
    string? SerialNumber,
    string? Manufacturer
);

public record CreateAssetRequest(
    string AssetCode,
    string AssetName,
    string Category,
    string Location,
    string PurchaseDate,
    decimal PurchaseCost,
    string? WarrantyExpiryDate,
    string? SerialNumber,
    string? Manufacturer
);

public record UpdateAssetRequest(
    string AssetName,
    string Category,
    string Location,
    string PurchaseDate,
    decimal PurchaseCost,
    string? WarrantyExpiryDate,
    string? SerialNumber,
    string? Manufacturer,
    string? Status = null
);

public record WorkOrderDto(
    long Id,
    string WorkOrderNumber,
    long? AssetId,
    string? AssetName,
    long? RoomId,
    string? RoomNumber,
    string DepartmentCode,
    string IssueDescription,
    string Priority,
    string Status,
    long? AssignedToUserId,
    string? AssignedToUserName,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt,
    string? ResolutionNotes,
    decimal TotalCost
);

public record CreateWorkOrderRequest(
    long? AssetId,
    long? RoomId,
    string DepartmentCode,
    string IssueDescription,
    string Priority,
    long? AssignedToUserId
);

public record UpdateWorkOrderStatusRequest(
    string Status,
    string? ResolutionNotes,
    decimal AdditionalCost
);

public record UpdateWorkOrderRequest(
    string DepartmentCode,
    string IssueDescription,
    string Priority,
    long? AssignedToUserId,
    string? ResolutionNotes,
    decimal TotalCost = 0
);

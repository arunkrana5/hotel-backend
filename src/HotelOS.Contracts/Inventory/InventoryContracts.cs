namespace HotelOS.Contracts.Inventory;

public record InventoryItemDto(
    long Id,
    string ItemCode,
    string ItemName,
    string Category,
    string UnitOfMeasure,
    decimal UnitCost,
    decimal CurrentStock,
    decimal ReorderLevel,
    decimal MinimumStock,
    decimal MaximumStock,
    string? LocationBin
);

public record CreateInventoryItemRequest(
    string ItemCode,
    string ItemName,
    string Category,
    string UnitOfMeasure,
    decimal UnitCost,
    decimal CurrentStock,
    decimal ReorderLevel,
    decimal MinimumStock,
    decimal MaximumStock,
    string? LocationBin
);

public record UpdateInventoryItemRequest(
    string ItemName,
    string Category,
    string UnitOfMeasure,
    decimal UnitCost,
    decimal ReorderLevel,
    decimal MinimumStock,
    decimal MaximumStock,
    string? LocationBin
);

public record UpdateStockLevelRequest(
    decimal QuantityChange,
    string TransactionType, // RECEIPT, ISSUE, ADJUSTMENT
    string? DepartmentCode,
    string? ReferenceNumber,
    string? Notes
);

public record StockTransactionDto(
    long Id,
    string TransactionNumber,
    long InventoryItemId,
    string ItemCode,
    string ItemName,
    string TransactionType,
    decimal Quantity,
    decimal UnitCost,
    decimal TotalValue,
    string? DepartmentCode,
    string? ReferenceNumber,
    DateTimeOffset TransactionDate,
    string? Notes
);

public record RecordStockTransactionRequest(
    long InventoryItemId,
    string TransactionType,
    decimal Quantity,
    decimal UnitCost,
    string? DepartmentCode,
    string? ReferenceNumber,
    string? Notes
);

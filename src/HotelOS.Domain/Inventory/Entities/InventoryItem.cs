namespace HotelOS.Domain.Inventory.Entities;

using HotelOS.Domain.Common;

public class InventoryItem : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = "FOOD_BEVERAGE"; // FOOD_BEVERAGE, HOUSEKEEPING_CHEMICALS, GUEST_SUPPLIES, SPARES, STATIONERY
    public string UnitOfMeasure { get; set; } = "PCS"; // KG, LTR, PCS, PACK, BOX
    public decimal UnitCost { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }
    public string? LocationBin { get; set; }
}

public class StockTransaction : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string TransactionNumber { get; set; } = string.Empty;
    public long InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public string TransactionType { get; set; } = "RECEIPT"; // RECEIPT, ISSUE, ADJUSTMENT, RETURN
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalValue { get; set; }
    public string? DepartmentCode { get; set; } // Department issued to (FB, HK, ENG, etc.)
    public string? ReferenceNumber { get; set; } // PO # or Issue Req #
    public DateTimeOffset TransactionDate { get; set; } = DateTimeOffset.UtcNow;
    public string? Notes { get; set; }
}

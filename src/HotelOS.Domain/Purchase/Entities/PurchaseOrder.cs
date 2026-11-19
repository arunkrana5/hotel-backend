namespace HotelOS.Domain.Purchase.Entities;

using HotelOS.Domain.Common;

public class PurchaseOrder : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string OrderNumber { get; set; } = string.Empty;
    public long? PurchaseRequisitionId { get; set; }
    public PurchaseRequisition? PurchaseRequisition { get; set; }
    public long VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public DateOnly OrderDate { get; set; }
    public DateOnly ExpectedDeliveryDate { get; set; }
    public string Status { get; set; } = "ISSUED"; // DRAFT, ISSUED, RECEIVED, CANCELLED
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}

public class PurchaseOrderItem : BaseEntity
{
    public long PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public string Unit { get; set; } = "PCS";
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

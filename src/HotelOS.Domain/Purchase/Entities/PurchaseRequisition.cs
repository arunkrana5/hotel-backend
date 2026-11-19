namespace HotelOS.Domain.Purchase.Entities;

using HotelOS.Domain.Common;
using HotelOS.Domain.Identity.Entities;

public class PurchaseRequisition : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string RequisitionNumber { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = "KIT";
    public long RequestedByUserId { get; set; }
    public User? RequestedByUser { get; set; }
    public DateOnly RequiredByDate { get; set; }
    public string Priority { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, URGENT
    public string Status { get; set; } = "DRAFT"; // DRAFT, PENDING_APPROVAL, APPROVED, REJECTED, ORDERED
    public decimal TotalEstimatedAmount { get; set; }
    public string? Justification { get; set; }
    public ICollection<PurchaseRequisitionItem> Items { get; set; } = new List<PurchaseRequisitionItem>();
}

public class PurchaseRequisitionItem : BaseEntity
{
    public long PurchaseRequisitionId { get; set; }
    public PurchaseRequisition? PurchaseRequisition { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "PCS";
    public decimal EstimatedUnitPrice { get; set; }
    public decimal TotalEstimatedPrice { get; set; }
}

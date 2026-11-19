using HotelOS.Domain.Common;

namespace HotelOS.Domain.Pms.Entities;

public class FolioItem : BaseEntity
{
    public long FolioId { get; set; }
    public Folio Folio { get; set; } = null!;

    public string ItemType { get; set; } = "CHARGE"; // CHARGE, PAYMENT, TAX, DISCOUNT
    public string DepartmentCode { get; set; } = "FO"; // FO, FB, SPA, MNT
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal Amount { get; set; }
    public string? PaymentModeCode { get; set; } // CC, CSH, CORP, UPI, WIRE
    public string? TransactionReference { get; set; }
    public DateTimeOffset PostedAt { get; set; } = DateTimeOffset.UtcNow;
    public string PostedBy { get; set; } = "SYSTEM";
}

namespace HotelOS.Domain.Purchase.Entities;

using HotelOS.Domain.Common;

public class Vendor : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string VendorCode { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, BLACKLISTED
}

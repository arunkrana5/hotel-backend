namespace HotelOS.Domain.Housekeeping.Entities;

using HotelOS.Domain.Common;

public class LaundryOrderItem : BaseEntity
{
    public long LaundryOrderId { get; set; }
    public LaundryOrder? LaundryOrder { get; set; }
    public string ServiceType { get; set; } = "WASH_AND_FOLD";
    public string GarmentName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Remarks { get; set; }
}

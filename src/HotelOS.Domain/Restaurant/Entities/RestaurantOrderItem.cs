namespace HotelOS.Domain.Restaurant.Entities;

using HotelOS.Domain.Common;

public class RestaurantOrderItem : BaseEntity
{
    public long RestaurantOrderId { get; set; }
    public RestaurantOrder? RestaurantOrder { get; set; }
    public long MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "QUEUED"; // QUEUED, PREPARING, READY, SERVED
    public string? Notes { get; set; }
}

namespace HotelOS.Domain.Restaurant.Entities;

using HotelOS.Domain.Common;

public class DiningTable : BaseEntity
{
    public long PropertyId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public string Section { get; set; } = "MAIN_DINING";
    public int Capacity { get; set; } = 4;
    public string Status { get; set; } = "AVAILABLE";
    public int DisplayOrder { get; set; }
}

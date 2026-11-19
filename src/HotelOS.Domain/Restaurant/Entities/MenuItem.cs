namespace HotelOS.Domain.Restaurant.Entities;

using HotelOS.Domain.Common;

public class MenuItem : BaseEntity
{
    public long PropertyId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = "MAIN_COURSE";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int PreparationTimeMinutes { get; set; } = 15;
    public string DietaryType { get; set; } = "VEG";
    public bool IsAvailable { get; set; } = true;
    public string? ImageUrl { get; set; }
}

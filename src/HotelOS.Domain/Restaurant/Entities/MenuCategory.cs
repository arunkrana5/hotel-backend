namespace HotelOS.Domain.Restaurant.Entities;

using HotelOS.Domain.Common;

public class MenuCategory : BaseEntity
{
    public long PropertyId { get; set; }
    public long? MenuId { get; set; }
    public Menu? Menu { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 1;
    public bool IsGuestVisible { get; set; } = true;
    public string ApplicableOutlets { get; set; } = "ROOM_SERVICE,MAIN_RESTAURANT";

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}

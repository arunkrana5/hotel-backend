namespace HotelOS.Domain.Restaurant.Entities;

using HotelOS.Domain.Common;

public class Menu : BaseEntity
{
    public long PropertyId { get; set; }
    public string MenuCode { get; set; } = string.Empty;
    public string MenuName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string MenuType { get; set; } = "ALL_DAY"; // ALL_DAY, BREAKFAST, LUNCH, DINNER, LATE_NIGHT, SPECIAL
    public string Status { get; set; } = "PUBLISHED"; // DRAFT, PUBLISHED, UNPUBLISHED, SCHEDULED, ARCHIVED
    public bool IsGuestVisible { get; set; } = true;
    public int DisplayPriority { get; set; } = 1;
    public string AvailableDays { get; set; } = "MON,TUE,WED,THU,FRI,SAT,SUN";
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public DateTimeOffset? EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveUntil { get; set; }
    public string ApplicableOutlets { get; set; } = "ROOM_SERVICE,MAIN_RESTAURANT";

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
    public ICollection<MenuCategory> Categories { get; set; } = new List<MenuCategory>();
}

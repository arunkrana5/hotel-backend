namespace HotelOS.Domain.Restaurant.Entities;

using HotelOS.Domain.Common;

public class MenuItem : BaseEntity
{
    public long PropertyId { get; set; }
    public long? MenuId { get; set; }
    public Menu? Menu { get; set; }
    public long? MenuCategoryId { get; set; }
    public MenuCategory? MenuCategory { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = "MAIN_COURSE";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal TaxPercent { get; set; } = 5.0m;
    public int PreparationTimeMinutes { get; set; } = 15;
    public string DietaryType { get; set; } = "VEG"; // VEG, NON_VEG, VEGAN, EGG
    public bool IsAvailable { get; set; } = true;
    public bool IsGuestVisible { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public string Status { get; set; } = "PUBLISHED"; // DRAFT, PUBLISHED, ARCHIVED
    public string? ImageUrl { get; set; }
    public string? Allergens { get; set; } // e.g. "Dairy, Gluten, Nuts, Shellfish"
    public string? VariantsJson { get; set; } // JSON array of variant options
    public string? AddOnsJson { get; set; } // JSON array of available add-ons
    public string ApplicableOutlets { get; set; } = "ROOM_SERVICE,MAIN_RESTAURANT";
}

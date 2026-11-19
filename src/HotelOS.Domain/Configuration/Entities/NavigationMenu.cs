using HotelOS.Domain.Common;

namespace HotelOS.Domain.Configuration.Entities;

public class NavigationMenu : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public long? ParentId { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public string? RequiredPermission { get; set; }
    public string? RequiredRole { get; set; }
    public string Module { get; set; } = string.Empty;
    public string? BadgeText { get; set; }

    public NavigationMenu? Parent { get; set; }
    public ICollection<NavigationMenu> Children { get; set; } = new List<NavigationMenu>();
}

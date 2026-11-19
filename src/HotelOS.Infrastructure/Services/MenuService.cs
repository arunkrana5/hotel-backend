using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Configuration;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly HotelOSDbContext _context;

    public MenuService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<List<NavigationMenuDto>> GetAuthorizedMenusAsync(
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default)
    {
        bool isSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
        var permSet = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);

        var allMenus = await _context.NavigationMenus
            .AsNoTracking()
            .Where(m => m.IsActive && !m.IsDeleted)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        // Filter authorized menus
        var authorized = allMenus.Where(m =>
        {
            if (isSuperAdmin) return true;

            // If required permission is specified, check permission
            if (!string.IsNullOrEmpty(m.RequiredPermission) && !permSet.Contains(m.RequiredPermission))
                return false;

            return true;
        }).ToList();

        // Build hierarchical tree
        var parentMenus = authorized.Where(m => m.ParentId == null).OrderBy(m => m.DisplayOrder).ToList();
        var result = new List<NavigationMenuDto>();

        foreach (var parent in parentMenus)
        {
            var children = authorized
                .Where(m => m.ParentId == parent.Id)
                .OrderBy(m => m.DisplayOrder)
                .Select(c => new NavigationMenuDto(
                    c.Id,
                    c.Code,
                    c.Title,
                    c.Path,
                    c.Icon,
                    c.DisplayOrder,
                    c.Module,
                    c.BadgeText,
                    new List<NavigationMenuDto>()
                ))
                .ToList();

            // If parent has required permission or children, or is general
            result.Add(new NavigationMenuDto(
                parent.Id,
                parent.Code,
                parent.Title,
                parent.Path,
                parent.Icon,
                parent.DisplayOrder,
                parent.Module,
                parent.BadgeText,
                children
            ));
        }

        return result;
    }
}

using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Restaurant;
using HotelOS.Domain.Pms.Entities;
using HotelOS.Domain.Restaurant.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class RestaurantService : IRestaurantService
{
    private readonly HotelOSDbContext _context;

    public RestaurantService(HotelOSDbContext context)
    {
        _context = context;
    }

    #region Menus Management

    public async Task<List<MenuDto>> GetMenusAsync(
        long propertyId = 1,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Menus
            .Include(m => m.Items.Where(i => !i.IsDeleted))
            .Include(m => m.Categories.Where(c => !c.IsDeleted))
            .AsNoTracking()
            .Where(m => m.PropertyId == propertyId && !m.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status) && status.Trim().ToUpperInvariant() != "ALL")
        {
            query = query.Where(m => m.Status == status.Trim().ToUpperInvariant());
        }

        var menus = await query
            .OrderBy(m => m.DisplayPriority)
            .ThenBy(m => m.MenuName)
            .ToListAsync(cancellationToken);

        // If no menus exist for this property, seed a default all-day in-room dining menu
        if (menus.Count == 0 && (string.IsNullOrWhiteSpace(status) || status.Trim().ToUpperInvariant() == "ALL"))
        {
            var defaultMenu = new Menu
            {
                PropertyId = propertyId,
                MenuCode = "MNU-ALLDAY",
                MenuName = "All-Day In-Room Dining",
                Description = "24/7 in-room dining featuring artisanal appetizers, gourmet mains, signature desserts, and cocktails.",
                MenuType = "ALL_DAY",
                Status = "PUBLISHED",
                IsGuestVisible = true,
                DisplayPriority = 1,
                AvailableDays = "MON,TUE,WED,THU,FRI,SAT,SUN",
                ApplicableOutlets = "ROOM_SERVICE,MAIN_RESTAURANT",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "SYSTEM"
            };

            _context.Menus.Add(defaultMenu);
            await _context.SaveChangesAsync(cancellationToken);

            // Backfill existing items with this menuId
            var existingItems = await _context.MenuItems
                .Where(m => m.PropertyId == propertyId && m.MenuId == null && !m.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var item in existingItems)
            {
                item.MenuId = defaultMenu.Id;
                item.IsGuestVisible = true;
                item.Status = "PUBLISHED";
            }
            await _context.SaveChangesAsync(cancellationToken);

            return await GetMenusAsync(propertyId, status, cancellationToken);
        }

        return menus.Select(m => new MenuDto(
            m.Id,
            m.PropertyId,
            m.MenuCode,
            m.MenuName,
            m.Description,
            m.ImageUrl,
            m.MenuType,
            m.Status,
            m.IsGuestVisible,
            m.DisplayPriority,
            m.AvailableDays,
            m.StartTime?.ToString(@"hh\:mm"),
            m.EndTime?.ToString(@"hh\:mm"),
            m.EffectiveFrom,
            m.EffectiveUntil,
            m.ApplicableOutlets,
            m.Items.Count,
            m.Categories.Count
        )).ToList();
    }

    public async Task<MenuDto?> GetMenuByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var menu = await _context.Menus
            .Include(m => m.Items.Where(i => !i.IsDeleted))
            .Include(m => m.Categories.Where(c => !c.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);

        if (menu == null) return null;

        return new MenuDto(
            menu.Id,
            menu.PropertyId,
            menu.MenuCode,
            menu.MenuName,
            menu.Description,
            menu.ImageUrl,
            menu.MenuType,
            menu.Status,
            menu.IsGuestVisible,
            menu.DisplayPriority,
            menu.AvailableDays,
            menu.StartTime?.ToString(@"hh\:mm"),
            menu.EndTime?.ToString(@"hh\:mm"),
            menu.EffectiveFrom,
            menu.EffectiveUntil,
            menu.ApplicableOutlets,
            menu.Items.Count,
            menu.Categories.Count
        );
    }

    public async Task<MenuDto> CreateMenuAsync(
        CreateMenuRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        TimeSpan? startTime = null;
        if (!string.IsNullOrWhiteSpace(request.StartTime) && TimeSpan.TryParse(request.StartTime, out var st))
            startTime = st;

        TimeSpan? endTime = null;
        if (!string.IsNullOrWhiteSpace(request.EndTime) && TimeSpan.TryParse(request.EndTime, out var et))
            endTime = et;

        var menu = new Menu
        {
            PropertyId = propertyId,
            MenuCode = request.MenuCode.Trim().ToUpperInvariant(),
            MenuName = request.MenuName.Trim(),
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            MenuType = request.MenuType.Trim().ToUpperInvariant(),
            Status = request.Status.Trim().ToUpperInvariant(),
            IsGuestVisible = request.IsGuestVisible,
            DisplayPriority = request.DisplayPriority,
            AvailableDays = !string.IsNullOrWhiteSpace(request.AvailableDays) ? request.AvailableDays.Trim() : "MON,TUE,WED,THU,FRI,SAT,SUN",
            StartTime = startTime,
            EndTime = endTime,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveUntil = request.EffectiveUntil,
            ApplicableOutlets = !string.IsNullOrWhiteSpace(request.ApplicableOutlets) ? request.ApplicableOutlets.Trim() : "ROOM_SERVICE,MAIN_RESTAURANT",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.Menus.Add(menu);
        await _context.SaveChangesAsync(cancellationToken);

        return new MenuDto(
            menu.Id,
            menu.PropertyId,
            menu.MenuCode,
            menu.MenuName,
            menu.Description,
            menu.ImageUrl,
            menu.MenuType,
            menu.Status,
            menu.IsGuestVisible,
            menu.DisplayPriority,
            menu.AvailableDays,
            menu.StartTime?.ToString(@"hh\:mm"),
            menu.EndTime?.ToString(@"hh\:mm"),
            menu.EffectiveFrom,
            menu.EffectiveUntil,
            menu.ApplicableOutlets,
            0,
            0
        );
    }

    public async Task<MenuDto?> UpdateMenuAsync(
        long id,
        UpdateMenuRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var menu = await _context.Menus
            .Include(m => m.Items.Where(i => !i.IsDeleted))
            .Include(m => m.Categories.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);

        if (menu == null) return null;

        TimeSpan? startTime = null;
        if (!string.IsNullOrWhiteSpace(request.StartTime) && TimeSpan.TryParse(request.StartTime, out var st))
            startTime = st;

        TimeSpan? endTime = null;
        if (!string.IsNullOrWhiteSpace(request.EndTime) && TimeSpan.TryParse(request.EndTime, out var et))
            endTime = et;

        menu.MenuName = request.MenuName.Trim();
        menu.Description = request.Description?.Trim();
        menu.ImageUrl = request.ImageUrl?.Trim();
        menu.MenuType = request.MenuType.Trim().ToUpperInvariant();
        menu.Status = request.Status.Trim().ToUpperInvariant();
        menu.IsGuestVisible = request.IsGuestVisible;
        menu.DisplayPriority = request.DisplayPriority;
        if (!string.IsNullOrWhiteSpace(request.AvailableDays)) menu.AvailableDays = request.AvailableDays.Trim();
        menu.StartTime = startTime;
        menu.EndTime = endTime;
        menu.EffectiveFrom = request.EffectiveFrom;
        menu.EffectiveUntil = request.EffectiveUntil;
        if (!string.IsNullOrWhiteSpace(request.ApplicableOutlets)) menu.ApplicableOutlets = request.ApplicableOutlets.Trim();

        menu.ModifiedAt = DateTimeOffset.UtcNow;
        menu.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new MenuDto(
            menu.Id,
            menu.PropertyId,
            menu.MenuCode,
            menu.MenuName,
            menu.Description,
            menu.ImageUrl,
            menu.MenuType,
            menu.Status,
            menu.IsGuestVisible,
            menu.DisplayPriority,
            menu.AvailableDays,
            menu.StartTime?.ToString(@"hh\:mm"),
            menu.EndTime?.ToString(@"hh\:mm"),
            menu.EffectiveFrom,
            menu.EffectiveUntil,
            menu.ApplicableOutlets,
            menu.Items.Count,
            menu.Categories.Count
        );
    }

    public async Task<MenuDto?> ToggleMenuStatusAsync(
        long id,
        string status,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var menu = await _context.Menus
            .Include(m => m.Items.Where(i => !i.IsDeleted))
            .Include(m => m.Categories.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);

        if (menu == null) return null;

        menu.Status = status.Trim().ToUpperInvariant();
        menu.ModifiedAt = DateTimeOffset.UtcNow;
        menu.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new MenuDto(
            menu.Id,
            menu.PropertyId,
            menu.MenuCode,
            menu.MenuName,
            menu.Description,
            menu.ImageUrl,
            menu.MenuType,
            menu.Status,
            menu.IsGuestVisible,
            menu.DisplayPriority,
            menu.AvailableDays,
            menu.StartTime?.ToString(@"hh\:mm"),
            menu.EndTime?.ToString(@"hh\:mm"),
            menu.EffectiveFrom,
            menu.EffectiveUntil,
            menu.ApplicableOutlets,
            menu.Items.Count,
            menu.Categories.Count
        );
    }

    public async Task<MenuDto?> DuplicateMenuAsync(
        long id,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var source = await _context.Menus
            .Include(m => m.Items.Where(i => !i.IsDeleted))
            .Include(m => m.Categories.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);

        if (source == null) return null;

        var newCode = $"{source.MenuCode}-COPY-{DateTimeOffset.UtcNow:MMddHHmm}";
        var duplicate = new Menu
        {
            PropertyId = source.PropertyId,
            MenuCode = newCode,
            MenuName = $"{source.MenuName} (Copy)",
            Description = source.Description,
            ImageUrl = source.ImageUrl,
            MenuType = source.MenuType,
            Status = "DRAFT",
            IsGuestVisible = false,
            DisplayPriority = source.DisplayPriority + 1,
            AvailableDays = source.AvailableDays,
            StartTime = source.StartTime,
            EndTime = source.EndTime,
            EffectiveFrom = source.EffectiveFrom,
            EffectiveUntil = source.EffectiveUntil,
            ApplicableOutlets = source.ApplicableOutlets,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.Menus.Add(duplicate);
        await _context.SaveChangesAsync(cancellationToken);

        // Duplicate items
        foreach (var item in source.Items)
        {
            var newItem = new MenuItem
            {
                PropertyId = item.PropertyId,
                MenuId = duplicate.Id,
                ItemCode = $"{item.ItemCode}-C{DateTimeOffset.UtcNow:fff}",
                ItemName = item.ItemName,
                Category = item.Category,
                Description = item.Description,
                Price = item.Price,
                TaxPercent = item.TaxPercent,
                PreparationTimeMinutes = item.PreparationTimeMinutes,
                DietaryType = item.DietaryType,
                IsAvailable = item.IsAvailable,
                IsGuestVisible = item.IsGuestVisible,
                IsFeatured = item.IsFeatured,
                DisplayOrder = item.DisplayOrder,
                Status = "DRAFT",
                ImageUrl = item.ImageUrl,
                Allergens = item.Allergens,
                VariantsJson = item.VariantsJson,
                AddOnsJson = item.AddOnsJson,
                ApplicableOutlets = item.ApplicableOutlets,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = createdBy
            };
            _context.MenuItems.Add(newItem);
        }
        await _context.SaveChangesAsync(cancellationToken);

        return await GetMenuByIdAsync(duplicate.Id, cancellationToken);
    }

    public async Task<bool> DeleteMenuAsync(
        long id,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        var menu = await _context.Menus.FindAsync([id], cancellationToken);
        if (menu == null || menu.IsDeleted) return false;

        menu.IsDeleted = true;
        menu.DeletedAt = DateTimeOffset.UtcNow;
        menu.DeletedBy = deletedBy;

        // Unlink associated items rather than deleting them
        var items = await _context.MenuItems.Where(i => i.MenuId == id).ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            item.MenuId = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    #endregion

    #region Categories Management

    public async Task<List<MenuCategoryDto>> GetCategoriesAsync(
        long propertyId = 1,
        long? menuId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MenuCategories
            .Include(c => c.Menu)
            .Include(c => c.Items.Where(i => !i.IsDeleted))
            .AsNoTracking()
            .Where(c => c.PropertyId == propertyId && !c.IsDeleted);

        if (menuId.HasValue)
        {
            query = query.Where(c => c.MenuId == menuId.Value);
        }

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.CategoryName)
            .ToListAsync(cancellationToken);

        // If no categories in table yet, auto-seed standard categories based on existing items
        if (categories.Count == 0 && !menuId.HasValue)
        {
            var seedCategories = new[]
            {
                new { Code = "APPETIZERS", Name = "Appetizers & Starters", Order = 1 },
                new { Code = "MAINS", Name = "Signature Mains", Order = 2 },
                new { Code = "BREADS", Name = "Artisanal Breads & Pizzas", Order = 3 },
                new { Code = "DESSERTS", Name = "Decadent Desserts", Order = 4 },
                new { Code = "COCKTAILS", Name = "Cocktails & Spirits", Order = 5 },
                new { Code = "BEVERAGES", Name = "Beverages & Refreshers", Order = 6 },
            };

            foreach (var sc in seedCategories)
            {
                _context.MenuCategories.Add(new MenuCategory
                {
                    PropertyId = propertyId,
                    CategoryCode = sc.Code,
                    CategoryName = sc.Name,
                    DisplayOrder = sc.Order,
                    IsGuestVisible = true,
                    ApplicableOutlets = "ROOM_SERVICE,MAIN_RESTAURANT",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "SYSTEM"
                });
            }
            await _context.SaveChangesAsync(cancellationToken);

            return await GetCategoriesAsync(propertyId, null, cancellationToken);
        }

        return categories.Select(c => new MenuCategoryDto(
            c.Id,
            c.PropertyId,
            c.MenuId,
            c.Menu?.MenuName,
            c.CategoryCode,
            c.CategoryName,
            c.Description,
            c.ImageUrl,
            c.DisplayOrder,
            c.IsGuestVisible,
            c.ApplicableOutlets,
            c.Items.Count
        )).ToList();
    }

    public async Task<MenuCategoryDto> CreateCategoryAsync(
        CreateMenuCategoryRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var category = new MenuCategory
        {
            PropertyId = propertyId,
            MenuId = request.MenuId,
            CategoryCode = request.CategoryCode.Trim().ToUpperInvariant(),
            CategoryName = request.CategoryName.Trim(),
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsGuestVisible = request.IsGuestVisible,
            ApplicableOutlets = !string.IsNullOrWhiteSpace(request.ApplicableOutlets) ? request.ApplicableOutlets.Trim() : "ROOM_SERVICE,MAIN_RESTAURANT",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.MenuCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        string? menuName = null;
        if (request.MenuId.HasValue)
        {
            var menu = await _context.Menus.FindAsync([request.MenuId.Value], cancellationToken);
            menuName = menu?.MenuName;
        }

        return new MenuCategoryDto(
            category.Id,
            category.PropertyId,
            category.MenuId,
            menuName,
            category.CategoryCode,
            category.CategoryName,
            category.Description,
            category.ImageUrl,
            category.DisplayOrder,
            category.IsGuestVisible,
            category.ApplicableOutlets,
            0
        );
    }

    public async Task<MenuCategoryDto?> UpdateCategoryAsync(
        long id,
        UpdateMenuCategoryRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var category = await _context.MenuCategories
            .Include(c => c.Menu)
            .Include(c => c.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (category == null) return null;

        category.MenuId = request.MenuId;
        category.CategoryName = request.CategoryName.Trim();
        category.Description = request.Description?.Trim();
        category.ImageUrl = request.ImageUrl?.Trim();
        category.DisplayOrder = request.DisplayOrder;
        category.IsGuestVisible = request.IsGuestVisible;
        if (!string.IsNullOrWhiteSpace(request.ApplicableOutlets)) category.ApplicableOutlets = request.ApplicableOutlets.Trim();

        category.ModifiedAt = DateTimeOffset.UtcNow;
        category.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new MenuCategoryDto(
            category.Id,
            category.PropertyId,
            category.MenuId,
            category.Menu?.MenuName,
            category.CategoryCode,
            category.CategoryName,
            category.Description,
            category.ImageUrl,
            category.DisplayOrder,
            category.IsGuestVisible,
            category.ApplicableOutlets,
            category.Items.Count
        );
    }

    public async Task<bool> DeleteCategoryAsync(
        long id,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        var category = await _context.MenuCategories.FindAsync([id], cancellationToken);
        if (category == null || category.IsDeleted) return false;

        category.IsDeleted = true;
        category.DeletedAt = DateTimeOffset.UtcNow;
        category.DeletedBy = deletedBy;

        var items = await _context.MenuItems.Where(i => i.MenuCategoryId == id).ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            item.MenuCategoryId = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    #endregion

    #region Tables

    public async Task<List<DiningTableDto>> GetTablesAsync(
        long propertyId = 1,
        string? section = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DiningTables
            .AsNoTracking()
            .Where(t => t.PropertyId == propertyId && !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(section))
            query = query.Where(t => t.Section == section.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status.Trim().ToUpperInvariant());

        return await query
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.TableNumber)
            .Select(t => new DiningTableDto(
                t.Id,
                t.TableNumber,
                t.Section,
                t.Capacity,
                t.Status,
                t.DisplayOrder
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<DiningTableDto?> UpdateTableStatusAsync(
        long tableId,
        UpdateTableStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var table = await _context.DiningTables.FindAsync([tableId], cancellationToken);
        if (table == null) return null;

        table.Status = request.Status.Trim().ToUpperInvariant();
        table.ModifiedAt = DateTimeOffset.UtcNow;
        table.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new DiningTableDto(
            table.Id,
            table.TableNumber,
            table.Section,
            table.Capacity,
            table.Status,
            table.DisplayOrder
        );
    }

    #endregion

    #region Menu Items

    public async Task<List<MenuItemDto>> GetMenuItemsAsync(
        long propertyId = 1,
        string? category = null,
        bool? isAvailableOnly = null,
        long? menuId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MenuItems
            .Include(m => m.Menu)
            .Include(m => m.MenuCategory)
            .AsNoTracking()
            .Where(m => m.PropertyId == propertyId && !m.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category) && category.Trim().ToUpperInvariant() != "ALL")
            query = query.Where(m => m.Category == category.Trim().ToUpperInvariant());

        if (isAvailableOnly == true)
            query = query.Where(m => m.IsAvailable);

        if (menuId.HasValue)
            query = query.Where(m => m.MenuId == menuId.Value);

        return await query
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.Category)
            .ThenBy(m => m.ItemName)
            .Select(m => new MenuItemDto(
                m.Id,
                m.ItemCode,
                m.ItemName,
                m.Category,
                m.Description,
                m.Price,
                m.PreparationTimeMinutes,
                m.DietaryType,
                m.IsAvailable,
                m.ImageUrl,
                m.MenuId,
                m.Menu != null ? m.Menu.MenuName : null,
                m.MenuCategoryId,
                m.MenuCategory != null ? m.MenuCategory.CategoryName : null,
                m.IsGuestVisible,
                m.IsFeatured,
                m.DisplayOrder,
                m.TaxPercent,
                m.Status,
                m.Allergens,
                m.VariantsJson,
                m.AddOnsJson,
                m.ApplicableOutlets
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<MenuItemDto> CreateMenuItemAsync(
        CreateMenuItemRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var item = new MenuItem
        {
            PropertyId = propertyId,
            MenuId = request.MenuId,
            MenuCategoryId = request.MenuCategoryId,
            ItemCode = request.ItemCode.Trim().ToUpperInvariant(),
            ItemName = request.ItemName.Trim(),
            Category = request.Category.Trim().ToUpperInvariant(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            TaxPercent = request.TaxPercent > 0 ? request.TaxPercent : 5.0m,
            PreparationTimeMinutes = request.PreparationTimeMinutes,
            DietaryType = request.DietaryType.Trim().ToUpperInvariant(),
            IsAvailable = request.IsAvailable,
            IsGuestVisible = request.IsGuestVisible,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder,
            Status = !string.IsNullOrWhiteSpace(request.Status) ? request.Status.Trim().ToUpperInvariant() : "PUBLISHED",
            ImageUrl = request.ImageUrl?.Trim(),
            Allergens = request.Allergens?.Trim(),
            VariantsJson = request.VariantsJson,
            AddOnsJson = request.AddOnsJson,
            ApplicableOutlets = !string.IsNullOrWhiteSpace(request.ApplicableOutlets) ? request.ApplicableOutlets.Trim() : "ROOM_SERVICE,MAIN_RESTAURANT",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        string? menuName = null;
        if (request.MenuId.HasValue)
        {
            var menu = await _context.Menus.FindAsync([request.MenuId.Value], cancellationToken);
            menuName = menu?.MenuName;
        }

        string? catName = null;
        if (request.MenuCategoryId.HasValue)
        {
            var cat = await _context.MenuCategories.FindAsync([request.MenuCategoryId.Value], cancellationToken);
            catName = cat?.CategoryName;
        }

        return new MenuItemDto(
            item.Id,
            item.ItemCode,
            item.ItemName,
            item.Category,
            item.Description,
            item.Price,
            item.PreparationTimeMinutes,
            item.DietaryType,
            item.IsAvailable,
            item.ImageUrl,
            item.MenuId,
            menuName,
            item.MenuCategoryId,
            catName,
            item.IsGuestVisible,
            item.IsFeatured,
            item.DisplayOrder,
            item.TaxPercent,
            item.Status,
            item.Allergens,
            item.VariantsJson,
            item.AddOnsJson,
            item.ApplicableOutlets
        );
    }

    public async Task<MenuItemDto?> UpdateMenuItemAsync(
        long id,
        UpdateMenuItemRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.MenuItems
            .Include(m => m.Menu)
            .Include(m => m.MenuCategory)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);

        if (item == null) return null;

        if (!string.IsNullOrWhiteSpace(request.ItemName)) item.ItemName = request.ItemName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Category)) item.Category = request.Category.Trim().ToUpperInvariant();
        item.Description = request.Description?.Trim();
        if (request.Price >= 0) item.Price = request.Price;
        if (request.TaxPercent >= 0) item.TaxPercent = request.TaxPercent;
        if (request.PreparationTimeMinutes >= 0) item.PreparationTimeMinutes = request.PreparationTimeMinutes;
        if (!string.IsNullOrWhiteSpace(request.DietaryType)) item.DietaryType = request.DietaryType.Trim().ToUpperInvariant();
        item.IsAvailable = request.IsAvailable;
        item.IsGuestVisible = request.IsGuestVisible;
        item.IsFeatured = request.IsFeatured;
        item.DisplayOrder = request.DisplayOrder;
        if (!string.IsNullOrWhiteSpace(request.Status)) item.Status = request.Status.Trim().ToUpperInvariant();
        if (request.ImageUrl != null) item.ImageUrl = request.ImageUrl.Trim();
        item.MenuId = request.MenuId;
        item.MenuCategoryId = request.MenuCategoryId;
        item.Allergens = request.Allergens?.Trim();
        item.VariantsJson = request.VariantsJson;
        item.AddOnsJson = request.AddOnsJson;
        if (!string.IsNullOrWhiteSpace(request.ApplicableOutlets)) item.ApplicableOutlets = request.ApplicableOutlets.Trim();

        item.ModifiedAt = DateTimeOffset.UtcNow;
        item.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new MenuItemDto(
            item.Id,
            item.ItemCode,
            item.ItemName,
            item.Category,
            item.Description,
            item.Price,
            item.PreparationTimeMinutes,
            item.DietaryType,
            item.IsAvailable,
            item.ImageUrl,
            item.MenuId,
            item.Menu?.MenuName,
            item.MenuCategoryId,
            item.MenuCategory?.CategoryName,
            item.IsGuestVisible,
            item.IsFeatured,
            item.DisplayOrder,
            item.TaxPercent,
            item.Status,
            item.Allergens,
            item.VariantsJson,
            item.AddOnsJson,
            item.ApplicableOutlets
        );
    }

    public async Task<MenuItemDto?> ToggleMenuItemAvailabilityAsync(
        long menuItemId,
        UpdateMenuItemAvailabilityRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.MenuItems
            .Include(m => m.Menu)
            .Include(m => m.MenuCategory)
            .FirstOrDefaultAsync(m => m.Id == menuItemId && !m.IsDeleted, cancellationToken);

        if (item == null) return null;

        item.IsAvailable = request.IsAvailable;
        item.ModifiedAt = DateTimeOffset.UtcNow;
        item.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new MenuItemDto(
            item.Id,
            item.ItemCode,
            item.ItemName,
            item.Category,
            item.Description,
            item.Price,
            item.PreparationTimeMinutes,
            item.DietaryType,
            item.IsAvailable,
            item.ImageUrl,
            item.MenuId,
            item.Menu?.MenuName,
            item.MenuCategoryId,
            item.MenuCategory?.CategoryName,
            item.IsGuestVisible,
            item.IsFeatured,
            item.DisplayOrder,
            item.TaxPercent,
            item.Status,
            item.Allergens,
            item.VariantsJson,
            item.AddOnsJson,
            item.ApplicableOutlets
        );
    }

    public async Task<bool> DeleteMenuItemAsync(
        long id,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.MenuItems.FindAsync([id], cancellationToken);
        if (item == null || item.IsDeleted) return false;

        item.IsDeleted = true;
        item.DeletedAt = DateTimeOffset.UtcNow;
        item.DeletedBy = deletedBy;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    #endregion

    #region Orders

    public async Task<PagedResult<RestaurantOrderDto>> GetOrdersPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? orderType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Room)
            .Include(o => o.Guest)
            .Include(o => o.ServerUser)
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.PropertyId == propertyId && !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(orderType))
            query = query.Where(o => o.OrderType == orderType.Trim().ToUpperInvariant());

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.PlacedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = orders.Select(o => new RestaurantOrderDto(
            o.Id,
            o.OrderNumber,
            o.OrderType,
            o.TableId,
            o.Table?.TableNumber,
            o.RoomId,
            o.Room?.RoomNumber,
            o.GuestId,
            o.Guest != null ? $"{o.Guest.FirstName} {o.Guest.LastName}" : o.GuestDisplayName,
            o.FolioId,
            o.Status,
            o.SubTotal,
            o.TaxAmount,
            o.DiscountAmount,
            o.TotalAmount,
            o.PaymentStatus,
            o.PaymentMethod,
            o.ServerUserId,
            o.ServerUser?.FullName,
            o.SpecialInstructions,
            o.PlacedAt,
            o.PreparedAt,
            o.ServedAt,
            o.Items.Select(i => new RestaurantOrderItemDto(
                i.Id,
                i.MenuItemId,
                i.ItemName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Status,
                i.Notes,
                i.SelectedVariantsJson,
                i.SelectedAddOnsJson,
                i.BasePrice,
                i.OptionsPrice
            )).ToList(),
            o.DeliveredAt,
            o.Source,
            o.DeliveryContactPhone,
            o.GuestDisplayName,
            o.IsStayVerified,
            o.EstimatedDeliveryMinutes,
            o.DeliveryStaffName
        )).ToList();

        return new PagedResult<RestaurantOrderDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<RestaurantOrderDto>> GetActiveKdsTicketsAsync(
        long propertyId = 1,
        CancellationToken cancellationToken = default)
    {
        var activeStatuses = new[] { "PLACED", "PREPARING", "READY" };

        var orders = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Room)
            .Include(o => o.Guest)
            .Include(o => o.Items.Where(i => i.Status != "SERVED" && i.Status != "CANCELLED"))
            .AsNoTracking()
            .Where(o => o.PropertyId == propertyId && activeStatuses.Contains(o.Status) && !o.IsDeleted)
            .OrderBy(o => o.PlacedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(o => new RestaurantOrderDto(
            o.Id,
            o.OrderNumber,
            o.OrderType,
            o.TableId,
            o.Table?.TableNumber,
            o.RoomId,
            o.Room?.RoomNumber,
            o.GuestId,
            o.Guest != null ? $"{o.Guest.FirstName} {o.Guest.LastName}" : o.GuestDisplayName,
            o.FolioId,
            o.Status,
            o.SubTotal,
            o.TaxAmount,
            o.DiscountAmount,
            o.TotalAmount,
            o.PaymentStatus,
            o.PaymentMethod,
            o.ServerUserId,
            o.ServerUser?.FullName,
            o.SpecialInstructions,
            o.PlacedAt,
            o.PreparedAt,
            o.ServedAt,
            o.Items.Select(i => new RestaurantOrderItemDto(
                i.Id,
                i.MenuItemId,
                i.ItemName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Status,
                i.Notes,
                i.SelectedVariantsJson,
                i.SelectedAddOnsJson,
                i.BasePrice,
                i.OptionsPrice
            )).ToList(),
            o.DeliveredAt,
            o.Source,
            o.DeliveryContactPhone,
            o.GuestDisplayName,
            o.IsStayVerified,
            o.EstimatedDeliveryMinutes,
            o.DeliveryStaffName
        )).ToList();
    }

    public async Task<RestaurantOrderDto> CreateOrderAsync(
        CreateRestaurantOrderRequest request,
        long propertyId,
        long? serverUserId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var countToday = await _context.RestaurantOrders
            .CountAsync(o => o.PropertyId == propertyId && o.PlacedAt.Date == DateTimeOffset.UtcNow.Date, cancellationToken);

        var prefix = request.OrderType.Trim().ToUpperInvariant() == "ROOM_SERVICE" ? "RS" : "POS";
        var orderNumber = $"{prefix}-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";

        decimal subtotal = 0;
        var orderItems = new List<RestaurantOrderItem>();

        foreach (var itemReq in request.Items)
        {
            var menuItem = await _context.MenuItems.FindAsync([itemReq.MenuItemId], cancellationToken)
                ?? throw new KeyNotFoundException($"Menu Item with ID {itemReq.MenuItemId} not found.");

            var unitPrice = menuItem.Price + itemReq.OptionsPrice;
            var itemTotal = itemReq.Quantity * unitPrice;
            subtotal += itemTotal;

            orderItems.Add(new RestaurantOrderItem
            {
                MenuItemId = menuItem.Id,
                ItemName = menuItem.ItemName,
                Quantity = itemReq.Quantity,
                UnitPrice = unitPrice,
                BasePrice = menuItem.Price,
                OptionsPrice = itemReq.OptionsPrice,
                TotalPrice = itemTotal,
                Status = "QUEUED",
                Notes = itemReq.Notes,
                SelectedVariantsJson = itemReq.SelectedVariantsJson,
                SelectedAddOnsJson = itemReq.SelectedAddOnsJson,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = createdBy
            });
        }

        var tax = Math.Round(subtotal * 0.05m, 2); // 5% F&B VAT/Tax
        var total = subtotal + tax;

        // Auto-link Folio if Room Service and room is occupied
        long? folioId = null;
        long? guestId = request.GuestId;

        if (request.RoomId.HasValue)
        {
            var activeRes = await _context.Reservations
                .Include(r => r.Folios)
                .Where(r => r.AssignedRoomId == request.RoomId.Value && r.ReservationStatus == "CHKI" && !r.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (activeRes != null)
            {
                guestId ??= activeRes.GuestId;
                var masterFolio = activeRes.Folios.FirstOrDefault(f => f.Status == "Open");
                folioId = masterFolio?.Id;
            }
        }

        var order = new RestaurantOrder
        {
            PropertyId = propertyId,
            OrderNumber = orderNumber,
            OrderType = request.OrderType.Trim().ToUpperInvariant(),
            TableId = request.TableId,
            RoomId = request.RoomId,
            GuestId = guestId,
            FolioId = folioId,
            Status = "PLACED",
            SubTotal = subtotal,
            TaxAmount = tax,
            DiscountAmount = 0,
            TotalAmount = total,
            PaymentStatus = "UNPAID",
            ServerUserId = serverUserId,
            SpecialInstructions = request.SpecialInstructions,
            PlacedAt = DateTimeOffset.UtcNow,
            Items = orderItems,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        if (request.TableId.HasValue)
        {
            var table = await _context.DiningTables.FindAsync([request.TableId.Value], cancellationToken);
            if (table != null)
            {
                table.Status = "OCCUPIED";
                table.ModifiedAt = DateTimeOffset.UtcNow;
                table.ModifiedBy = createdBy;
            }
        }

        _context.RestaurantOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        string? tableNumber = null;
        if (order.TableId.HasValue)
        {
            var tbl = await _context.DiningTables.FindAsync([order.TableId.Value], cancellationToken);
            tableNumber = tbl?.TableNumber;
        }

        string? roomNumber = null;
        if (order.RoomId.HasValue)
        {
            var rm = await _context.Rooms.FindAsync([order.RoomId.Value], cancellationToken);
            roomNumber = rm?.RoomNumber;
        }

        return new RestaurantOrderDto(
            order.Id,
            order.OrderNumber,
            order.OrderType,
            order.TableId,
            tableNumber,
            order.RoomId,
            roomNumber,
            order.GuestId,
            null,
            order.FolioId,
            order.Status,
            order.SubTotal,
            order.TaxAmount,
            order.DiscountAmount,
            order.TotalAmount,
            order.PaymentStatus,
            order.PaymentMethod,
            order.ServerUserId,
            null,
            order.SpecialInstructions,
            order.PlacedAt,
            order.PreparedAt,
            order.ServedAt,
            order.Items.Select(i => new RestaurantOrderItemDto(
                i.Id,
                i.MenuItemId,
                i.ItemName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Status,
                i.Notes,
                i.SelectedVariantsJson,
                i.SelectedAddOnsJson,
                i.BasePrice,
                i.OptionsPrice
            )).ToList(),
            order.DeliveredAt,
            order.Source,
            order.DeliveryContactPhone,
            order.GuestDisplayName,
            order.IsStayVerified,
            order.EstimatedDeliveryMinutes,
            order.DeliveryStaffName
        );
    }

    public async Task<RestaurantOrderDto?> UpdateOrderStatusAsync(
        long orderId,
        UpdateOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Room)
            .Include(o => o.Guest)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

        if (order == null) return null;

        var targetStatus = request.Status.Trim().ToUpperInvariant();
        order.Status = targetStatus;
        order.ModifiedAt = DateTimeOffset.UtcNow;
        order.ModifiedBy = updatedBy;

        if (targetStatus == "PREPARING")
        {
            foreach (var item in order.Items.Where(i => i.Status == "QUEUED"))
            {
                item.Status = "PREPARING";
                item.ModifiedAt = DateTimeOffset.UtcNow;
                item.ModifiedBy = updatedBy;
            }
        }
        else if (targetStatus == "READY")
        {
            order.PreparedAt = DateTimeOffset.UtcNow;
            foreach (var item in order.Items.Where(i => i.Status == "PREPARING" || i.Status == "QUEUED"))
            {
                item.Status = "READY";
                item.ModifiedAt = DateTimeOffset.UtcNow;
                item.ModifiedBy = updatedBy;
            }
        }
        else if (targetStatus == "SERVED" || targetStatus == "DELIVERED")
        {
            order.ServedAt = DateTimeOffset.UtcNow;
            order.DeliveredAt = DateTimeOffset.UtcNow;
            foreach (var item in order.Items)
            {
                item.Status = "SERVED";
                item.ModifiedAt = DateTimeOffset.UtcNow;
                item.ModifiedBy = updatedBy;
            }

            if (order.TableId.HasValue && order.PaymentStatus == "PAID")
            {
                var tbl = await _context.DiningTables.FindAsync([order.TableId.Value], cancellationToken);
                if (tbl != null)
                {
                    tbl.Status = "DIRTY";
                    tbl.ModifiedAt = DateTimeOffset.UtcNow;
                    tbl.ModifiedBy = updatedBy;
                }
            }
        }
        else if (targetStatus == "CANCELLED")
        {
            foreach (var item in order.Items)
            {
                item.Status = "CANCELLED";
                item.ModifiedAt = DateTimeOffset.UtcNow;
                item.ModifiedBy = updatedBy;
            }

            if (order.TableId.HasValue)
            {
                var tbl = await _context.DiningTables.FindAsync([order.TableId.Value], cancellationToken);
                if (tbl != null)
                {
                    tbl.Status = "AVAILABLE";
                    tbl.ModifiedAt = DateTimeOffset.UtcNow;
                    tbl.ModifiedBy = updatedBy;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new RestaurantOrderDto(
            order.Id,
            order.OrderNumber,
            order.OrderType,
            order.TableId,
            order.Table?.TableNumber,
            order.RoomId,
            order.Room?.RoomNumber,
            order.GuestId,
            order.Guest != null ? $"{order.Guest.FirstName} {order.Guest.LastName}" : order.GuestDisplayName,
            order.FolioId,
            order.Status,
            order.SubTotal,
            order.TaxAmount,
            order.DiscountAmount,
            order.TotalAmount,
            order.PaymentStatus,
            order.PaymentMethod,
            order.ServerUserId,
            null,
            order.SpecialInstructions,
            order.PlacedAt,
            order.PreparedAt,
            order.ServedAt,
            order.Items.Select(i => new RestaurantOrderItemDto(
                i.Id,
                i.MenuItemId,
                i.ItemName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Status,
                i.Notes,
                i.SelectedVariantsJson,
                i.SelectedAddOnsJson,
                i.BasePrice,
                i.OptionsPrice
            )).ToList(),
            order.DeliveredAt,
            order.Source,
            order.DeliveryContactPhone,
            order.GuestDisplayName,
            order.IsStayVerified,
            order.EstimatedDeliveryMinutes,
            order.DeliveryStaffName
        );
    }

    public async Task<RestaurantOrderItemDto?> UpdateOrderItemStatusAsync(
        long orderItemId,
        UpdateOrderItemStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.RestaurantOrderItems
            .Include(i => i.RestaurantOrder)
            .FirstOrDefaultAsync(i => i.Id == orderItemId, cancellationToken);

        if (item == null) return null;

        item.Status = request.Status.Trim().ToUpperInvariant();
        item.ModifiedAt = DateTimeOffset.UtcNow;
        item.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new RestaurantOrderItemDto(
            item.Id,
            item.MenuItemId,
            item.ItemName,
            item.Quantity,
            item.UnitPrice,
            item.TotalPrice,
            item.Status,
            item.Notes,
            item.SelectedVariantsJson,
            item.SelectedAddOnsJson,
            item.BasePrice,
            item.OptionsPrice
        );
    }

    public async Task<RestaurantOrderDto?> SettleOrderAsync(
        long orderId,
        SettleRestaurantOrderRequest request,
        string settledBy,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Room)
            .Include(o => o.Guest)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

        if (order == null) return null;

        order.PaymentStatus = request.PaymentStatus.Trim().ToUpperInvariant();
        order.PaymentMethod = request.PaymentMethod?.Trim().ToUpperInvariant();
        order.ModifiedAt = DateTimeOffset.UtcNow;
        order.ModifiedBy = settledBy;

        if (order.PaymentMethod == "CHARGE_TO_ROOM")
        {
            if (!order.FolioId.HasValue)
            {
                throw new InvalidOperationException("Cannot charge to room: No active open folio found for this room stay.");
            }

            var existingCharge = await _context.FolioItems
                .AnyAsync(f => f.FolioId == order.FolioId.Value && f.TransactionReference == order.OrderNumber, cancellationToken);

            if (!existingCharge)
            {
                var folioCharge = new FolioItem
                {
                    FolioId = order.FolioId.Value,
                    ItemType = "CHARGE",
                    DepartmentCode = "FB",
                    Description = $"Restaurant Order #{order.OrderNumber}",
                    UnitPrice = order.TotalAmount,
                    Quantity = 1,
                    Amount = order.TotalAmount,
                    TransactionReference = order.OrderNumber,
                    PostedAt = DateTimeOffset.UtcNow,
                    PostedBy = settledBy,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = settledBy
                };
                _context.FolioItems.Add(folioCharge);
            }
        }

        if (order.TableId.HasValue && (order.Status == "SERVED" || order.Status == "DELIVERED"))
        {
            var tbl = await _context.DiningTables.FindAsync([order.TableId.Value], cancellationToken);
            if (tbl != null)
            {
                tbl.Status = "DIRTY";
                tbl.ModifiedAt = DateTimeOffset.UtcNow;
                tbl.ModifiedBy = settledBy;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new RestaurantOrderDto(
            order.Id,
            order.OrderNumber,
            order.OrderType,
            order.TableId,
            order.Table?.TableNumber,
            order.RoomId,
            order.Room?.RoomNumber,
            order.GuestId,
            order.Guest != null ? $"{order.Guest.FirstName} {order.Guest.LastName}" : order.GuestDisplayName,
            order.FolioId,
            order.Status,
            order.SubTotal,
            order.TaxAmount,
            order.DiscountAmount,
            order.TotalAmount,
            order.PaymentStatus,
            order.PaymentMethod,
            order.ServerUserId,
            null,
            order.SpecialInstructions,
            order.PlacedAt,
            order.PreparedAt,
            order.ServedAt,
            order.Items.Select(i => new RestaurantOrderItemDto(
                i.Id,
                i.MenuItemId,
                i.ItemName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Status,
                i.Notes,
                i.SelectedVariantsJson,
                i.SelectedAddOnsJson,
                i.BasePrice,
                i.OptionsPrice
            )).ToList(),
            order.DeliveredAt,
            order.Source,
            order.DeliveryContactPhone,
            order.GuestDisplayName,
            order.IsStayVerified,
            order.EstimatedDeliveryMinutes,
            order.DeliveryStaffName
        );
    }

    #endregion
}

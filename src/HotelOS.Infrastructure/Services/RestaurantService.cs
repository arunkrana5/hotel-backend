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

    public async Task<List<MenuItemDto>> GetMenuItemsAsync(
        long propertyId = 1,
        string? category = null,
        bool? isAvailableOnly = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MenuItems
            .AsNoTracking()
            .Where(m => m.PropertyId == propertyId && !m.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(m => m.Category == category.Trim().ToUpperInvariant());

        if (isAvailableOnly == true)
            query = query.Where(m => m.IsAvailable);

        return await query
            .OrderBy(m => m.Category)
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
                m.ImageUrl
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
            ItemCode = request.ItemCode.Trim().ToUpperInvariant(),
            ItemName = request.ItemName,
            Category = request.Category.Trim().ToUpperInvariant(),
            Description = request.Description,
            Price = request.Price,
            PreparationTimeMinutes = request.PreparationTimeMinutes,
            DietaryType = request.DietaryType.Trim().ToUpperInvariant(),
            IsAvailable = request.IsAvailable,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.MenuItems.Add(item);
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
            item.ImageUrl
        );
    }

    public async Task<MenuItemDto?> ToggleMenuItemAvailabilityAsync(
        long menuItemId,
        UpdateMenuItemAvailabilityRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.MenuItems.FindAsync([menuItemId], cancellationToken);
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
            item.ImageUrl
        );
    }

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

        var dtos = orders.Select(MapToOrderDto).ToList();

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
        var activeStatuses = new[] { "PLACED", "COOKING" };

        var orders = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Room)
            .Include(o => o.Guest)
            .Include(o => o.ServerUser)
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.PropertyId == propertyId && !o.IsDeleted && activeStatuses.Contains(o.Status))
            .OrderBy(o => o.PlacedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToOrderDto).ToList();
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

            var itemTotal = itemReq.Quantity * menuItem.Price;
            subtotal += itemTotal;

            orderItems.Add(new RestaurantOrderItem
            {
                MenuItemId = menuItem.Id,
                ItemName = menuItem.ItemName,
                Quantity = itemReq.Quantity,
                UnitPrice = menuItem.Price,
                TotalPrice = itemTotal,
                Status = "QUEUED",
                Notes = itemReq.Notes,
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

        // If table assigned, mark table as OCCUPIED
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

        return await GetOrderByIdAsync(order.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to load created order.");
    }

    public async Task<RestaurantOrderDto?> UpdateOrderStatusAsync(
        long orderId,
        UpdateOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null) return null;

        var newStatus = request.Status.Trim().ToUpperInvariant();
        order.Status = newStatus;
        order.ModifiedAt = DateTimeOffset.UtcNow;
        order.ModifiedBy = updatedBy;

        if (newStatus == "COOKING")
        {
            foreach (var item in order.Items.Where(i => i.Status == "QUEUED"))
            {
                item.Status = "PREPARING";
                item.ModifiedAt = DateTimeOffset.UtcNow;
                item.ModifiedBy = updatedBy;
            }
        }
        else if (newStatus == "READY")
        {
            order.PreparedAt = DateTimeOffset.UtcNow;
            foreach (var item in order.Items.Where(i => i.Status == "PREPARING" || i.Status == "QUEUED"))
            {
                item.Status = "READY";
                item.ModifiedAt = DateTimeOffset.UtcNow;
                item.ModifiedBy = updatedBy;
            }
        }
        else if (newStatus == "SERVED")
        {
            order.ServedAt = DateTimeOffset.UtcNow;
            foreach (var item in order.Items)
            {
                item.Status = "SERVED";
                item.ModifiedAt = DateTimeOffset.UtcNow;
                item.ModifiedBy = updatedBy;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetOrderByIdAsync(order.Id, cancellationToken);
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
            item.Notes
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
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null) return null;

        var paymentStatus = request.PaymentStatus.Trim().ToUpperInvariant();

        if (paymentStatus == "CHARGED_TO_ROOM")
        {
            // Must have FolioId or find it from Room
            long? folioId = order.FolioId;
            if (!folioId.HasValue && order.RoomId.HasValue)
            {
                var activeRes = await _context.Reservations
                    .Include(r => r.Folios)
                    .Where(r => r.AssignedRoomId == order.RoomId.Value && r.ReservationStatus == "CHKI" && !r.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                var masterFolio = activeRes?.Folios.FirstOrDefault(f => f.Status == "Open");
                folioId = masterFolio?.Id;
            }

            if (!folioId.HasValue)
            {
                throw new InvalidOperationException("Cannot charge to room: No active folio found for this guest/room.");
            }

            var folio = await _context.Folios.FindAsync([folioId.Value], cancellationToken)
                ?? throw new InvalidOperationException("Active folio not found.");

            var charge = new FolioItem
            {
                FolioId = folio.Id,
                ItemType = "CHARGE",
                DepartmentCode = "FB",
                Description = $"Restaurant Dining Bill ({order.OrderNumber})",
                UnitPrice = order.TotalAmount,
                Quantity = 1,
                Amount = order.TotalAmount,
                TransactionReference = order.OrderNumber,
                PostedAt = DateTimeOffset.UtcNow,
                PostedBy = settledBy
            };

            _context.FolioItems.Add(charge);
            folio.TotalCharges += order.TotalAmount;
            folio.BalanceAmount += order.TotalAmount;
            folio.ModifiedAt = DateTimeOffset.UtcNow;
            folio.ModifiedBy = settledBy;

            order.FolioId = folio.Id;
            order.PaymentStatus = "CHARGED_TO_ROOM";
            order.PaymentMethod = "ROOM_CHARGE";
        }
        else
        {
            order.PaymentStatus = "PAID";
            order.PaymentMethod = request.PaymentMethod?.Trim().ToUpperInvariant() ?? "CARD";
        }

        order.Status = "BILLED";
        order.SettledAt = DateTimeOffset.UtcNow;
        order.ModifiedAt = DateTimeOffset.UtcNow;
        order.ModifiedBy = settledBy;

        // Release dining table to AVAILABLE
        if (order.TableId.HasValue)
        {
            var table = await _context.DiningTables.FindAsync([order.TableId.Value], cancellationToken);
            if (table != null)
            {
                table.Status = "AVAILABLE";
                table.ModifiedAt = DateTimeOffset.UtcNow;
                table.ModifiedBy = settledBy;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetOrderByIdAsync(order.Id, cancellationToken);
    }

    private async Task<RestaurantOrderDto?> GetOrderByIdAsync(long id, CancellationToken cancellationToken)
    {
        var o = await _context.RestaurantOrders
            .Include(x => x.Table)
            .Include(x => x.Room)
            .Include(x => x.Guest)
            .Include(x => x.ServerUser)
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return o == null ? null : MapToOrderDto(o);
    }

    private static RestaurantOrderDto MapToOrderDto(RestaurantOrder o)
    {
        return new RestaurantOrderDto(
            o.Id,
            o.OrderNumber,
            o.OrderType,
            o.TableId,
            o.Table?.TableNumber,
            o.RoomId,
            o.Room?.RoomNumber,
            o.GuestId,
            o.Guest != null ? $"{o.Guest.FirstName} {o.Guest.LastName}" : null,
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
                i.Notes
            )).ToList()
        );
    }
}

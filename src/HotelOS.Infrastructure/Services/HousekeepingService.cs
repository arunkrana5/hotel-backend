using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Housekeeping;
using HotelOS.Domain.Housekeeping.Entities;
using HotelOS.Domain.Pms.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class HousekeepingService : IHousekeepingService
{
    private readonly HotelOSDbContext _context;

    public HousekeepingService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<HousekeepingTaskDto>> GetTasksPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? taskType = null,
        string? priority = null,
        long? roomId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.HousekeepingTasks
            .Include(t => t.Room)
            .Include(t => t.AssignedToUser)
            .Include(t => t.InspectedByUser)
            .AsNoTracking()
            .Where(t => t.PropertyId == propertyId && !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(taskType))
            query = query.Where(t => t.TaskType == taskType.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(priority))
            query = query.Where(t => t.Priority == priority.Trim().ToUpperInvariant());

        if (roomId.HasValue)
            query = query.Where(t => t.RoomId == roomId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new HousekeepingTaskDto(
                t.Id,
                t.TaskNumber,
                t.RoomId,
                t.Room != null ? t.Room.RoomNumber : string.Empty,
                t.Room != null ? t.Room.FloorNumber : 0,
                t.Room != null ? t.Room.RoomTypeCode : string.Empty,
                t.TaskType,
                t.Priority,
                t.Status,
                t.AssignedToUserId,
                t.AssignedToUser != null ? t.AssignedToUser.FullName : null,
                t.AssignedAt,
                t.StartedAt,
                t.CompletedAt,
                t.InspectedByUserId,
                t.InspectedByUser != null ? t.InspectedByUser.FullName : null,
                t.InspectedAt,
                t.Remarks,
                t.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<HousekeepingTaskDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<HousekeepingTaskDto> CreateTaskAsync(
        CreateHousekeepingTaskRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.PropertyId == propertyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Room with ID {request.RoomId} not found.");

        var countToday = await _context.HousekeepingTasks
            .CountAsync(t => t.PropertyId == propertyId && t.CreatedAt.Date == DateTimeOffset.UtcNow.Date, cancellationToken);

        var taskNumber = $"HK-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";

        var task = new HousekeepingTask
        {
            PropertyId = propertyId,
            TaskNumber = taskNumber,
            RoomId = request.RoomId,
            TaskType = request.TaskType.Trim().ToUpperInvariant(),
            Priority = request.Priority.Trim().ToUpperInvariant(),
            Status = "PENDING",
            AssignedToUserId = request.AssignedToUserId,
            AssignedAt = request.AssignedToUserId.HasValue ? DateTimeOffset.UtcNow : null,
            Remarks = request.Remarks,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.HousekeepingTasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        string? assignedName = null;
        if (task.AssignedToUserId.HasValue)
        {
            var user = await _context.Users.FindAsync([task.AssignedToUserId.Value], cancellationToken);
            assignedName = user?.FullName;
        }

        return new HousekeepingTaskDto(
            task.Id,
            task.TaskNumber,
            task.RoomId,
            room.RoomNumber,
            room.FloorNumber,
            room.RoomTypeCode,
            task.TaskType,
            task.Priority,
            task.Status,
            task.AssignedToUserId,
            assignedName,
            task.AssignedAt,
            task.StartedAt,
            task.CompletedAt,
            null,
            null,
            null,
            task.Remarks,
            task.CreatedAt
        );
    }

    public async Task<HousekeepingTaskDto?> UpdateTaskStatusAsync(
        long taskId,
        UpdateHousekeepingTaskStatusRequest request,
        long? currentUserId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var task = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .Include(t => t.AssignedToUser)
            .Include(t => t.InspectedByUser)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null) return null;

        var newStatus = request.Status.Trim().ToUpperInvariant();
        task.Status = newStatus;
        task.ModifiedAt = DateTimeOffset.UtcNow;
        task.ModifiedBy = updatedBy;

        if (request.Remarks != null)
            task.Remarks = request.Remarks;

        if (newStatus == "IN_PROGRESS" && task.StartedAt == null)
        {
            task.StartedAt = DateTimeOffset.UtcNow;
        }
        else if (newStatus == "COMPLETED" || newStatus == "CLEANED")
        {
            task.CompletedAt = DateTimeOffset.UtcNow;
            if (task.Room != null)
            {
                task.Room.RoomStatusCode = "CLN";
                task.Room.ModifiedAt = DateTimeOffset.UtcNow;
                task.Room.ModifiedBy = updatedBy;
            }
        }
        else if (newStatus == "INSPECTED")
        {
            task.InspectedAt = DateTimeOffset.UtcNow;
            task.InspectedByUserId = currentUserId;
            if (task.Room != null)
            {
                task.Room.RoomStatusCode = "INS";
                task.Room.ModifiedAt = DateTimeOffset.UtcNow;
                task.Room.ModifiedBy = updatedBy;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new HousekeepingTaskDto(
            task.Id,
            task.TaskNumber,
            task.RoomId,
            task.Room != null ? task.Room.RoomNumber : string.Empty,
            task.Room != null ? task.Room.FloorNumber : 0,
            task.Room != null ? task.Room.RoomTypeCode : string.Empty,
            task.TaskType,
            task.Priority,
            task.Status,
            task.AssignedToUserId,
            task.AssignedToUser?.FullName,
            task.AssignedAt,
            task.StartedAt,
            task.CompletedAt,
            task.InspectedByUserId,
            task.InspectedByUser?.FullName,
            task.InspectedAt,
            task.Remarks,
            task.CreatedAt
        );
    }

    public async Task<HousekeepingTaskDto?> AssignTaskAsync(
        long taskId,
        AssignHousekeepingTaskRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var task = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .Include(t => t.AssignedToUser)
            .Include(t => t.InspectedByUser)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null) return null;

        task.AssignedToUserId = request.AssignedToUserId;
        task.AssignedAt = DateTimeOffset.UtcNow;
        task.ModifiedAt = DateTimeOffset.UtcNow;
        task.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        var user = await _context.Users.FindAsync([request.AssignedToUserId], cancellationToken);

        return new HousekeepingTaskDto(
            task.Id,
            task.TaskNumber,
            task.RoomId,
            task.Room != null ? task.Room.RoomNumber : string.Empty,
            task.Room != null ? task.Room.FloorNumber : 0,
            task.Room != null ? task.Room.RoomTypeCode : string.Empty,
            task.TaskType,
            task.Priority,
            task.Status,
            task.AssignedToUserId,
            user?.FullName,
            task.AssignedAt,
            task.StartedAt,
            task.CompletedAt,
            task.InspectedByUserId,
            task.InspectedByUser?.FullName,
            task.InspectedAt,
            task.Remarks,
            task.CreatedAt
        );
    }

    public async Task<List<LinenItemDto>> GetLinenItemsAsync(
        long propertyId = 1,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LinenItems
            .AsNoTracking()
            .Where(l => l.PropertyId == propertyId && !l.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(l => l.Category == category.Trim().ToUpperInvariant());

        return await query
            .OrderBy(l => l.ItemName)
            .Select(l => new LinenItemDto(
                l.Id,
                l.ItemCode,
                l.ItemName,
                l.Category,
                l.ParLevel,
                l.CleanStock,
                l.InUseStock,
                l.InLaundryStock,
                l.DiscardedStock,
                l.Unit
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<LinenItemDto?> UpdateLinenStockAsync(
        long linenId,
        UpdateLinenStockRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.LinenItems.FindAsync([linenId], cancellationToken);
        if (item == null) return null;

        item.CleanStock = request.CleanStock;
        item.InUseStock = request.InUseStock;
        item.InLaundryStock = request.InLaundryStock;
        item.DiscardedStock = request.DiscardedStock;
        item.ModifiedAt = DateTimeOffset.UtcNow;
        item.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new LinenItemDto(
            item.Id,
            item.ItemCode,
            item.ItemName,
            item.Category,
            item.ParLevel,
            item.CleanStock,
            item.InUseStock,
            item.InLaundryStock,
            item.DiscardedStock,
            item.Unit
        );
    }

    public async Task<PagedResult<LaundryOrderDto>> GetLaundryOrdersPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? orderType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LaundryOrders
            .Include(o => o.Room)
            .Include(o => o.Guest)
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.PropertyId == propertyId && !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(orderType))
            query = query.Where(o => o.OrderType == orderType.Trim().ToUpperInvariant());

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = orders.Select(o => new LaundryOrderDto(
            o.Id,
            o.OrderNumber,
            o.OrderType,
            o.RoomId,
            o.Room?.RoomNumber,
            o.GuestId,
            o.Guest != null ? $"{o.Guest.FirstName} {o.Guest.LastName}" : null,
            o.FolioId,
            o.Status,
            o.TotalPieces,
            o.SubTotal,
            o.TaxAmount,
            o.TotalAmount,
            o.IsExpress,
            o.SpecialInstructions,
            o.DeliveredAt,
            o.CreatedAt,
            o.Items.Select(i => new LaundryOrderItemDto(
                i.Id,
                i.ServiceType,
                i.GarmentName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Remarks
            )).ToList()
        )).ToList();

        return new PagedResult<LaundryOrderDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<LaundryOrderDto> CreateLaundryOrderAsync(
        CreateLaundryOrderRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var countToday = await _context.LaundryOrders
            .CountAsync(o => o.PropertyId == propertyId && o.CreatedAt.Date == DateTimeOffset.UtcNow.Date, cancellationToken);

        var orderNumber = $"LND-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";

        decimal subtotal = 0;
        int totalPieces = 0;
        var orderItems = new List<LaundryOrderItem>();

        foreach (var itemReq in request.Items)
        {
            var itemTotal = itemReq.Quantity * itemReq.UnitPrice;
            subtotal += itemTotal;
            totalPieces += itemReq.Quantity;

            orderItems.Add(new LaundryOrderItem
            {
                ServiceType = itemReq.ServiceType.Trim().ToUpperInvariant(),
                GarmentName = itemReq.GarmentName,
                Quantity = itemReq.Quantity,
                UnitPrice = itemReq.UnitPrice,
                TotalPrice = itemTotal,
                Remarks = itemReq.Remarks,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = createdBy
            });
        }

        if (request.IsExpress)
        {
            subtotal *= 1.25m; // 25% express surcharge
        }

        var tax = Math.Round(subtotal * 0.12m, 2); // 12% standard GST/tax
        var total = subtotal + tax;

        // Auto-link active Folio if room is occupied
        long? folioId = null;
        if (request.RoomId.HasValue)
        {
            var activeRes = await _context.Reservations
                .Include(r => r.Folios)
                .Where(r => r.AssignedRoomId == request.RoomId.Value && r.ReservationStatus == "CHKI" && !r.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            var masterFolio = activeRes?.Folios.FirstOrDefault(f => f.Status == "Open");
            if (masterFolio != null)
            {
                folioId = masterFolio.Id;

                // Post charge directly to guest folio!
                var folioCharge = new FolioItem
                {
                    FolioId = masterFolio.Id,
                    ItemType = "CHARGE",
                    DepartmentCode = "HK",
                    Description = $"Guest Laundry Service ({orderNumber})",
                    UnitPrice = total,
                    Quantity = 1,
                    Amount = total,
                    TransactionReference = orderNumber,
                    PostedAt = DateTimeOffset.UtcNow,
                    PostedBy = createdBy
                };
                _context.FolioItems.Add(folioCharge);
                masterFolio.TotalCharges += total;
                masterFolio.BalanceAmount += total;
                masterFolio.ModifiedAt = DateTimeOffset.UtcNow;
                masterFolio.ModifiedBy = createdBy;
            }
        }

        var order = new LaundryOrder
        {
            PropertyId = propertyId,
            OrderNumber = orderNumber,
            OrderType = request.OrderType.Trim().ToUpperInvariant(),
            RoomId = request.RoomId,
            GuestId = request.GuestId,
            FolioId = folioId,
            Status = "RECEIVED",
            TotalPieces = totalPieces,
            SubTotal = subtotal,
            TaxAmount = tax,
            TotalAmount = total,
            IsExpress = request.IsExpress,
            SpecialInstructions = request.SpecialInstructions,
            Items = orderItems,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.LaundryOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        string? roomNumber = null;
        if (order.RoomId.HasValue)
        {
            var room = await _context.Rooms.FindAsync([order.RoomId.Value], cancellationToken);
            roomNumber = room?.RoomNumber;
        }

        string? guestName = null;
        if (order.GuestId.HasValue)
        {
            var guest = await _context.Guests.FindAsync([order.GuestId.Value], cancellationToken);
            guestName = guest != null ? $"{guest.FirstName} {guest.LastName}" : null;
        }

        return new LaundryOrderDto(
            order.Id,
            order.OrderNumber,
            order.OrderType,
            order.RoomId,
            roomNumber,
            order.GuestId,
            guestName,
            order.FolioId,
            order.Status,
            order.TotalPieces,
            order.SubTotal,
            order.TaxAmount,
            order.TotalAmount,
            order.IsExpress,
            order.SpecialInstructions,
            order.DeliveredAt,
            order.CreatedAt,
            order.Items.Select(i => new LaundryOrderItemDto(
                i.Id,
                i.ServiceType,
                i.GarmentName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Remarks
            )).ToList()
        );
    }

    public async Task<LaundryOrderDto?> UpdateLaundryStatusAsync(
        long orderId,
        UpdateLaundryStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.LaundryOrders
            .Include(o => o.Room)
            .Include(o => o.Guest)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null) return null;

        var newStatus = request.Status.Trim().ToUpperInvariant();
        order.Status = newStatus;
        order.ModifiedAt = DateTimeOffset.UtcNow;
        order.ModifiedBy = updatedBy;

        if (newStatus == "DELIVERED")
        {
            order.DeliveredAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new LaundryOrderDto(
            order.Id,
            order.OrderNumber,
            order.OrderType,
            order.RoomId,
            order.Room?.RoomNumber,
            order.GuestId,
            order.Guest != null ? $"{order.Guest.FirstName} {order.Guest.LastName}" : null,
            order.FolioId,
            order.Status,
            order.TotalPieces,
            order.SubTotal,
            order.TaxAmount,
            order.TotalAmount,
            order.IsExpress,
            order.SpecialInstructions,
            order.DeliveredAt,
            order.CreatedAt,
            order.Items.Select(i => new LaundryOrderItemDto(
                i.Id,
                i.ServiceType,
                i.GarmentName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Remarks
            )).ToList()
        );
    }
}

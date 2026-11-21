using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Maintenance;
using HotelOS.Domain.Maintenance.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly HotelOSDbContext _context;

    public MaintenanceService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AssetDto>> GetAssetsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? category = null,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Assets
            .AsNoTracking()
            .Where(a => a.PropertyId == propertyId && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(a => a.Category == category.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(a =>
                a.AssetCode.ToLower().Contains(s) ||
                a.AssetName.ToLower().Contains(s) ||
                a.Location.ToLower().Contains(s) ||
                (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(s)) ||
                (a.Manufacturer != null && a.Manufacturer.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(a => a.AssetCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AssetDto(
                a.Id,
                a.AssetCode,
                a.AssetName,
                a.Category,
                a.Location,
                a.PurchaseDate.ToString("yyyy-MM-dd"),
                a.PurchaseCost,
                a.WarrantyExpiryDate.HasValue ? a.WarrantyExpiryDate.Value.ToString("yyyy-MM-dd") : null,
                a.Status,
                a.SerialNumber,
                a.Manufacturer
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<AssetDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AssetDto> CreateAssetAsync(
        CreateAssetRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var purchaseDate = DateOnly.TryParse(request.PurchaseDate, out var pd)
            ? pd
            : DateOnly.FromDateTime(DateTime.UtcNow);

        DateOnly? warrantyDate = null;
        if (!string.IsNullOrWhiteSpace(request.WarrantyExpiryDate) && DateOnly.TryParse(request.WarrantyExpiryDate, out var wd))
            warrantyDate = wd;

        var asset = new Asset
        {
            PropertyId = propertyId,
            AssetCode = request.AssetCode.Trim().ToUpperInvariant(),
            AssetName = request.AssetName.Trim(),
            Category = request.Category.Trim().ToUpperInvariant(),
            Location = request.Location.Trim(),
            PurchaseDate = purchaseDate,
            PurchaseCost = request.PurchaseCost,
            WarrantyExpiryDate = warrantyDate,
            Status = "OPERATIONAL",
            SerialNumber = request.SerialNumber?.Trim(),
            Manufacturer = request.Manufacturer?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync(cancellationToken);

        return new AssetDto(
            asset.Id,
            asset.AssetCode,
            asset.AssetName,
            asset.Category,
            asset.Location,
            asset.PurchaseDate.ToString("yyyy-MM-dd"),
            asset.PurchaseCost,
            asset.WarrantyExpiryDate?.ToString("yyyy-MM-dd"),
            asset.Status,
            asset.SerialNumber,
            asset.Manufacturer
        );
    }

    public async Task<AssetDto?> UpdateAssetAsync(
        long id,
        UpdateAssetRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var asset = await _context.Assets.FindAsync([id], cancellationToken);
        if (asset == null) return null;

        if (!string.IsNullOrWhiteSpace(request.AssetName)) asset.AssetName = request.AssetName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Category)) asset.Category = request.Category.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(request.Location)) asset.Location = request.Location.Trim();
        if (DateOnly.TryParse(request.PurchaseDate, out var pd)) asset.PurchaseDate = pd;
        if (request.PurchaseCost >= 0) asset.PurchaseCost = request.PurchaseCost;
        if (!string.IsNullOrWhiteSpace(request.WarrantyExpiryDate) && DateOnly.TryParse(request.WarrantyExpiryDate, out var wd))
            asset.WarrantyExpiryDate = wd;
        if (request.SerialNumber != null) asset.SerialNumber = request.SerialNumber.Trim();
        if (request.Manufacturer != null) asset.Manufacturer = request.Manufacturer.Trim();
        if (!string.IsNullOrWhiteSpace(request.Status)) asset.Status = request.Status.Trim().ToUpperInvariant();

        asset.ModifiedAt = DateTimeOffset.UtcNow;
        asset.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new AssetDto(
            asset.Id,
            asset.AssetCode,
            asset.AssetName,
            asset.Category,
            asset.Location,
            asset.PurchaseDate.ToString("yyyy-MM-dd"),
            asset.PurchaseCost,
            asset.WarrantyExpiryDate?.ToString("yyyy-MM-dd"),
            asset.Status,
            asset.SerialNumber,
            asset.Manufacturer
        );
    }

    public async Task<PagedResult<WorkOrderDto>> GetWorkOrdersPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? priority = null,
        string? department = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkOrders
            .Include(w => w.Asset)
            .Include(w => w.Room)
            .Include(w => w.AssignedToUser)
            .AsNoTracking()
            .Where(w => w.PropertyId == propertyId && !w.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(w => w.Status == status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(priority))
            query = query.Where(w => w.Priority == priority.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(w => w.DepartmentCode == department.Trim().ToUpperInvariant());

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(w => w.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WorkOrderDto(
                w.Id,
                w.WorkOrderNumber,
                w.AssetId,
                w.Asset != null ? w.Asset.AssetName : null,
                w.RoomId,
                w.Room != null ? w.Room.RoomNumber : null,
                w.DepartmentCode,
                w.IssueDescription,
                w.Priority,
                w.Status,
                w.AssignedToUserId,
                w.AssignedToUser != null ? (w.AssignedToUser.FirstName + " " + w.AssignedToUser.LastName).Trim() : null,
                w.RequestedAt,
                w.CompletedAt,
                w.ResolutionNotes,
                w.TotalCost
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<WorkOrderDto> CreateWorkOrderAsync(
        CreateWorkOrderRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var countToday = await _context.WorkOrders.CountAsync(w => w.PropertyId == propertyId && w.CreatedAt >= DateTimeOffset.UtcNow.Date, cancellationToken);
        var workOrderNumber = $"WO-{DateTime.UtcNow:yyyyMMdd}-{(countToday + 1):D3}";

        var workOrder = new WorkOrder
        {
            PropertyId = propertyId,
            WorkOrderNumber = workOrderNumber,
            AssetId = request.AssetId,
            RoomId = request.RoomId,
            DepartmentCode = request.DepartmentCode.Trim().ToUpperInvariant(),
            IssueDescription = request.IssueDescription.Trim(),
            Priority = request.Priority.Trim().ToUpperInvariant(),
            Status = request.AssignedToUserId.HasValue ? "ASSIGNED" : "OPEN",
            AssignedToUserId = request.AssignedToUserId,
            RequestedAt = DateTimeOffset.UtcNow,
            TotalCost = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(cancellationToken);

        var asset = request.AssetId.HasValue ? await _context.Assets.FindAsync(new object[] { request.AssetId.Value }, cancellationToken) : null;
        var room = request.RoomId.HasValue ? await _context.Rooms.FindAsync(new object[] { request.RoomId.Value }, cancellationToken) : null;
        var user = request.AssignedToUserId.HasValue ? await _context.Users.FindAsync(new object[] { request.AssignedToUserId.Value }, cancellationToken) : null;

        return new WorkOrderDto(
            workOrder.Id,
            workOrder.WorkOrderNumber,
            workOrder.AssetId,
            asset?.AssetName,
            workOrder.RoomId,
            room?.RoomNumber,
            workOrder.DepartmentCode,
            workOrder.IssueDescription,
            workOrder.Priority,
            workOrder.Status,
            workOrder.AssignedToUserId,
            user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
            workOrder.RequestedAt,
            workOrder.CompletedAt,
            workOrder.ResolutionNotes,
            workOrder.TotalCost
        );
    }

    public async Task<WorkOrderDto?> UpdateWorkOrderAsync(
        long id,
        UpdateWorkOrderRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Asset)
            .Include(w => w.Room)
            .Include(w => w.AssignedToUser)
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);

        if (workOrder == null) return null;

        if (!string.IsNullOrWhiteSpace(request.DepartmentCode)) workOrder.DepartmentCode = request.DepartmentCode.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(request.IssueDescription)) workOrder.IssueDescription = request.IssueDescription.Trim();
        if (!string.IsNullOrWhiteSpace(request.Priority)) workOrder.Priority = request.Priority.Trim().ToUpperInvariant();
        workOrder.AssignedToUserId = request.AssignedToUserId;
        if (!string.IsNullOrWhiteSpace(request.ResolutionNotes)) workOrder.ResolutionNotes = request.ResolutionNotes.Trim();
        if (request.TotalCost >= 0) workOrder.TotalCost = request.TotalCost;

        workOrder.ModifiedAt = DateTimeOffset.UtcNow;
        workOrder.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new WorkOrderDto(
            workOrder.Id,
            workOrder.WorkOrderNumber,
            workOrder.AssetId,
            workOrder.Asset?.AssetName,
            workOrder.RoomId,
            workOrder.Room?.RoomNumber,
            workOrder.DepartmentCode,
            workOrder.IssueDescription,
            workOrder.Priority,
            workOrder.Status,
            workOrder.AssignedToUserId,
            workOrder.AssignedToUser != null ? $"{workOrder.AssignedToUser.FirstName} {workOrder.AssignedToUser.LastName}".Trim() : null,
            workOrder.RequestedAt,
            workOrder.CompletedAt,
            workOrder.ResolutionNotes,
            workOrder.TotalCost
        );
    }

    public async Task<WorkOrderDto?> UpdateWorkOrderStatusAsync(
        long workOrderId,
        UpdateWorkOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Asset)
            .Include(w => w.Room)
            .Include(w => w.AssignedToUser)
            .FirstOrDefaultAsync(w => w.Id == workOrderId && !w.IsDeleted, cancellationToken);

        if (workOrder == null) return null;

        var newStatus = request.Status.Trim().ToUpperInvariant();
        workOrder.Status = newStatus;
        if (!string.IsNullOrWhiteSpace(request.ResolutionNotes))
            workOrder.ResolutionNotes = request.ResolutionNotes.Trim();

        workOrder.TotalCost += request.AdditionalCost;

        if (newStatus == "COMPLETED")
        {
            workOrder.CompletedAt = DateTimeOffset.UtcNow;
            if (workOrder.Asset != null && workOrder.Asset.Status == "UNDER_MAINTENANCE")
            {
                workOrder.Asset.Status = "OPERATIONAL";
            }
        }
        else if (newStatus == "IN_PROGRESS" && workOrder.Asset != null)
        {
            workOrder.Asset.Status = "UNDER_MAINTENANCE";
        }

        workOrder.ModifiedAt = DateTimeOffset.UtcNow;
        workOrder.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new WorkOrderDto(
            workOrder.Id,
            workOrder.WorkOrderNumber,
            workOrder.AssetId,
            workOrder.Asset?.AssetName,
            workOrder.RoomId,
            workOrder.Room?.RoomNumber,
            workOrder.DepartmentCode,
            workOrder.IssueDescription,
            workOrder.Priority,
            workOrder.Status,
            workOrder.AssignedToUserId,
            workOrder.AssignedToUser != null ? $"{workOrder.AssignedToUser.FirstName} {workOrder.AssignedToUser.LastName}".Trim() : null,
            workOrder.RequestedAt,
            workOrder.CompletedAt,
            workOrder.ResolutionNotes,
            workOrder.TotalCost
        );
    }
}

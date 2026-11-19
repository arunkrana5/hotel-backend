using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Purchase;
using HotelOS.Domain.Purchase.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class PurchaseService : IPurchaseService
{
    private readonly HotelOSDbContext _context;

    public PurchaseService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<VendorDto>> GetVendorsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Vendors
            .AsNoTracking()
            .Where(v => v.PropertyId == propertyId && !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(v => v.VendorName.ToLower().Contains(s) || v.VendorCode.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(v => v.Status == status.Trim().ToUpperInvariant());

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(v => v.VendorName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VendorDto(
                v.Id,
                v.VendorCode,
                v.VendorName,
                v.ContactPerson,
                v.Email,
                v.Phone,
                v.Address,
                v.TaxNumber,
                v.PaymentTermsDays,
                v.Status
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<VendorDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<VendorDto> CreateVendorAsync(
        CreateVendorRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var vendor = new Vendor
        {
            PropertyId = propertyId,
            VendorCode = request.VendorCode.Trim().ToUpperInvariant(),
            VendorName = request.VendorName,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            TaxNumber = request.TaxNumber,
            PaymentTermsDays = request.PaymentTermsDays,
            Status = "ACTIVE",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync(cancellationToken);

        return new VendorDto(
            vendor.Id,
            vendor.VendorCode,
            vendor.VendorName,
            vendor.ContactPerson,
            vendor.Email,
            vendor.Phone,
            vendor.Address,
            vendor.TaxNumber,
            vendor.PaymentTermsDays,
            vendor.Status
        );
    }

    public async Task<PagedResult<PurchaseRequisitionDto>> GetRequisitionsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? department = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PurchaseRequisitions
            .Include(r => r.RequestedByUser)
            .Include(r => r.Items)
            .AsNoTracking()
            .Where(r => r.PropertyId == propertyId && !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(r => r.DepartmentCode == department.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status.Trim().ToUpperInvariant());

        var total = await query.CountAsync(cancellationToken);
        var reqs = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = reqs.Select(r => new PurchaseRequisitionDto(
            r.Id,
            r.RequisitionNumber,
            r.DepartmentCode,
            r.RequestedByUserId,
            r.RequestedByUser?.FullName,
            r.RequiredByDate.ToString("yyyy-MM-dd"),
            r.Priority,
            r.Status,
            r.TotalEstimatedAmount,
            r.Justification,
            r.CreatedAt,
            r.Items.Select(i => new PurchaseRequisitionItemDto(
                i.Id,
                i.ItemName,
                i.Description,
                i.Quantity,
                i.Unit,
                i.EstimatedUnitPrice,
                i.TotalEstimatedPrice
            )).ToList()
        )).ToList();

        return new PagedResult<PurchaseRequisitionDto> { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<PurchaseRequisitionDto> CreateRequisitionAsync(
        CreatePurchaseRequisitionRequest request,
        long propertyId,
        long userId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var countToday = await _context.PurchaseRequisitions
            .CountAsync(r => r.PropertyId == propertyId && r.CreatedAt.Date == DateTimeOffset.UtcNow.Date, cancellationToken);

        var reqNumber = $"PR-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";

        decimal total = 0;
        var items = new List<PurchaseRequisitionItem>();

        foreach (var itemReq in request.Items)
        {
            var itemTotal = itemReq.Quantity * itemReq.EstimatedUnitPrice;
            total += itemTotal;
            items.Add(new PurchaseRequisitionItem
            {
                ItemName = itemReq.ItemName,
                Description = itemReq.Description,
                Quantity = itemReq.Quantity,
                Unit = itemReq.Unit,
                EstimatedUnitPrice = itemReq.EstimatedUnitPrice,
                TotalEstimatedPrice = itemTotal,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = createdBy
            });
        }

        var req = new PurchaseRequisition
        {
            PropertyId = propertyId,
            RequisitionNumber = reqNumber,
            DepartmentCode = request.DepartmentCode.Trim().ToUpperInvariant(),
            RequestedByUserId = userId,
            RequiredByDate = DateOnly.Parse(request.RequiredByDate),
            Priority = request.Priority.Trim().ToUpperInvariant(),
            Status = "PENDING_APPROVAL",
            TotalEstimatedAmount = total,
            Justification = request.Justification,
            Items = items,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.PurchaseRequisitions.Add(req);
        await _context.SaveChangesAsync(cancellationToken);

        var user = await _context.Users.FindAsync([userId], cancellationToken);

        return new PurchaseRequisitionDto(
            req.Id,
            req.RequisitionNumber,
            req.DepartmentCode,
            req.RequestedByUserId,
            user?.FullName,
            req.RequiredByDate.ToString("yyyy-MM-dd"),
            req.Priority,
            req.Status,
            req.TotalEstimatedAmount,
            req.Justification,
            req.CreatedAt,
            items.Select(i => new PurchaseRequisitionItemDto(
                i.Id,
                i.ItemName,
                i.Description,
                i.Quantity,
                i.Unit,
                i.EstimatedUnitPrice,
                i.TotalEstimatedPrice
            )).ToList()
        );
    }

    public async Task<PurchaseRequisitionDto?> UpdateRequisitionStatusAsync(
        long requisitionId,
        UpdateRequisitionStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var req = await _context.PurchaseRequisitions
            .Include(r => r.RequestedByUser)
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == requisitionId, cancellationToken);

        if (req == null) return null;

        req.Status = request.Status.Trim().ToUpperInvariant();
        req.ModifiedAt = DateTimeOffset.UtcNow;
        req.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new PurchaseRequisitionDto(
            req.Id,
            req.RequisitionNumber,
            req.DepartmentCode,
            req.RequestedByUserId,
            req.RequestedByUser?.FullName,
            req.RequiredByDate.ToString("yyyy-MM-dd"),
            req.Priority,
            req.Status,
            req.TotalEstimatedAmount,
            req.Justification,
            req.CreatedAt,
            req.Items.Select(i => new PurchaseRequisitionItemDto(
                i.Id,
                i.ItemName,
                i.Description,
                i.Quantity,
                i.Unit,
                i.EstimatedUnitPrice,
                i.TotalEstimatedPrice
            )).ToList()
        );
    }

    public async Task<PagedResult<PurchaseOrderDto>> GetOrdersPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        long? vendorId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PurchaseOrders
            .Include(o => o.Vendor)
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.PropertyId == propertyId && !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status.Trim().ToUpperInvariant());

        if (vendorId.HasValue)
            query = query.Where(o => o.VendorId == vendorId.Value);

        var total = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = orders.Select(o => new PurchaseOrderDto(
            o.Id,
            o.OrderNumber,
            o.PurchaseRequisitionId,
            o.VendorId,
            o.Vendor != null ? o.Vendor.VendorName : string.Empty,
            o.OrderDate.ToString("yyyy-MM-dd"),
            o.ExpectedDeliveryDate.ToString("yyyy-MM-dd"),
            o.Status,
            o.SubTotal,
            o.TaxAmount,
            o.TotalAmount,
            o.PaymentTerms,
            o.Notes,
            o.CreatedAt,
            o.Items.Select(i => new PurchaseOrderItemDto(
                i.Id,
                i.ItemName,
                i.QuantityOrdered,
                i.QuantityReceived,
                i.Unit,
                i.UnitPrice,
                i.TotalPrice
            )).ToList()
        )).ToList();

        return new PagedResult<PurchaseOrderDto> { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<PurchaseOrderDto> CreateOrderAsync(
        CreatePurchaseOrderRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var vendor = await _context.Vendors.FindAsync([request.VendorId], cancellationToken)
            ?? throw new KeyNotFoundException($"Vendor #{request.VendorId} not found.");

        var countToday = await _context.PurchaseOrders
            .CountAsync(o => o.PropertyId == propertyId && o.CreatedAt.Date == DateTimeOffset.UtcNow.Date, cancellationToken);

        var poNumber = $"PO-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";

        decimal subtotal = 0;
        var items = new List<PurchaseOrderItem>();

        foreach (var itemReq in request.Items)
        {
            var itemTotal = itemReq.QuantityOrdered * itemReq.UnitPrice;
            subtotal += itemTotal;
            items.Add(new PurchaseOrderItem
            {
                ItemName = itemReq.ItemName,
                QuantityOrdered = itemReq.QuantityOrdered,
                QuantityReceived = 0,
                Unit = itemReq.Unit,
                UnitPrice = itemReq.UnitPrice,
                TotalPrice = itemTotal,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = createdBy
            });
        }

        var tax = Math.Round(subtotal * 0.10m, 2);
        var total = subtotal + tax;

        var po = new PurchaseOrder
        {
            PropertyId = propertyId,
            OrderNumber = poNumber,
            PurchaseRequisitionId = request.PurchaseRequisitionId,
            VendorId = request.VendorId,
            OrderDate = DateOnly.Parse(request.OrderDate),
            ExpectedDeliveryDate = DateOnly.Parse(request.ExpectedDeliveryDate),
            Status = "ISSUED",
            SubTotal = subtotal,
            TaxAmount = tax,
            TotalAmount = total,
            PaymentTerms = request.PaymentTerms ?? $"Net {vendor.PaymentTermsDays} Days",
            Notes = request.Notes,
            Items = items,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.PurchaseOrders.Add(po);
        await _context.SaveChangesAsync(cancellationToken);

        return new PurchaseOrderDto(
            po.Id,
            po.OrderNumber,
            po.PurchaseRequisitionId,
            po.VendorId,
            vendor.VendorName,
            po.OrderDate.ToString("yyyy-MM-dd"),
            po.ExpectedDeliveryDate.ToString("yyyy-MM-dd"),
            po.Status,
            po.SubTotal,
            po.TaxAmount,
            po.TotalAmount,
            po.PaymentTerms,
            po.Notes,
            po.CreatedAt,
            items.Select(i => new PurchaseOrderItemDto(
                i.Id,
                i.ItemName,
                i.QuantityOrdered,
                i.QuantityReceived,
                i.Unit,
                i.UnitPrice,
                i.TotalPrice
            )).ToList()
        );
    }

    public async Task<PurchaseOrderDto?> UpdateOrderStatusAsync(
        long orderId,
        UpdatePurchaseOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var po = await _context.PurchaseOrders
            .Include(o => o.Vendor)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (po == null) return null;

        po.Status = request.Status.Trim().ToUpperInvariant();
        po.ModifiedAt = DateTimeOffset.UtcNow;
        po.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new PurchaseOrderDto(
            po.Id,
            po.OrderNumber,
            po.PurchaseRequisitionId,
            po.VendorId,
            po.Vendor != null ? po.Vendor.VendorName : string.Empty,
            po.OrderDate.ToString("yyyy-MM-dd"),
            po.ExpectedDeliveryDate.ToString("yyyy-MM-dd"),
            po.Status,
            po.SubTotal,
            po.TaxAmount,
            po.TotalAmount,
            po.PaymentTerms,
            po.Notes,
            po.CreatedAt,
            po.Items.Select(i => new PurchaseOrderItemDto(
                i.Id,
                i.ItemName,
                i.QuantityOrdered,
                i.QuantityReceived,
                i.Unit,
                i.UnitPrice,
                i.TotalPrice
            )).ToList()
        );
    }
}

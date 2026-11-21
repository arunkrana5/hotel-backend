using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Inventory;
using HotelOS.Domain.Inventory.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly HotelOSDbContext _context;

    public InventoryService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<InventoryItemDto>> GetItemsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? category = null,
        bool? lowStockOnly = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.InventoryItems
            .AsNoTracking()
            .Where(i => i.PropertyId == propertyId && !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(i => i.ItemName.ToLower().Contains(s) || i.ItemCode.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(i => i.Category == category.Trim().ToUpperInvariant());

        if (lowStockOnly == true)
            query = query.Where(i => i.CurrentStock <= i.ReorderLevel);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(i => i.ItemName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InventoryItemDto(
                i.Id,
                i.ItemCode,
                i.ItemName,
                i.Category,
                i.UnitOfMeasure,
                i.UnitCost,
                i.CurrentStock,
                i.ReorderLevel,
                i.MinimumStock,
                i.MaximumStock,
                i.LocationBin
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<InventoryItemDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<InventoryItemDto> CreateItemAsync(
        CreateInventoryItemRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var item = new InventoryItem
        {
            PropertyId = propertyId,
            ItemCode = request.ItemCode.Trim().ToUpperInvariant(),
            ItemName = request.ItemName,
            Category = request.Category.Trim().ToUpperInvariant(),
            UnitOfMeasure = request.UnitOfMeasure.Trim().ToUpperInvariant(),
            UnitCost = request.UnitCost,
            CurrentStock = request.CurrentStock,
            ReorderLevel = request.ReorderLevel,
            MinimumStock = request.MinimumStock,
            MaximumStock = request.MaximumStock,
            LocationBin = request.LocationBin,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return new InventoryItemDto(
            item.Id,
            item.ItemCode,
            item.ItemName,
            item.Category,
            item.UnitOfMeasure,
            item.UnitCost,
            item.CurrentStock,
            item.ReorderLevel,
            item.MinimumStock,
            item.MaximumStock,
            item.LocationBin
        );
    }

    public async Task<InventoryItemDto> UpdateItemAsync(
        long id,
        UpdateInventoryItemRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.InventoryItems.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Inventory item #{id} not found.");

        item.ItemName = request.ItemName.Trim();
        item.Category = request.Category.Trim().ToUpperInvariant();
        item.UnitOfMeasure = request.UnitOfMeasure.Trim().ToUpperInvariant();
        item.UnitCost = request.UnitCost;
        item.ReorderLevel = request.ReorderLevel;
        item.MinimumStock = request.MinimumStock;
        item.MaximumStock = request.MaximumStock;
        item.LocationBin = request.LocationBin;
        item.ModifiedAt = DateTimeOffset.UtcNow;
        item.ModifiedBy = modifiedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new InventoryItemDto(
            item.Id,
            item.ItemCode,
            item.ItemName,
            item.Category,
            item.UnitOfMeasure,
            item.UnitCost,
            item.CurrentStock,
            item.ReorderLevel,
            item.MinimumStock,
            item.MaximumStock,
            item.LocationBin
        );
    }

    public async Task<StockTransactionDto> RecordTransactionAsync(
        RecordStockTransactionRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.InventoryItems.FindAsync([request.InventoryItemId], cancellationToken)
            ?? throw new KeyNotFoundException($"Inventory item #{request.InventoryItemId} not found.");

        var countToday = await _context.StockTransactions
            .CountAsync(t => t.PropertyId == propertyId && t.CreatedAt.Date == DateTimeOffset.UtcNow.Date, cancellationToken);

        var txnType = request.TransactionType.Trim().ToUpperInvariant();
        var prefix = txnType == "RECEIPT" ? "GRN" : txnType == "ISSUE" ? "ISS" : "ADJ";
        var txnNumber = $"{prefix}-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";

        if (txnType == "RECEIPT" || txnType == "RETURN")
        {
            item.CurrentStock += request.Quantity;
        }
        else if (txnType == "ISSUE")
        {
            if (item.CurrentStock < request.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for {item.ItemName}. Current stock is {item.CurrentStock} {item.UnitOfMeasure}.");
            }
            item.CurrentStock -= request.Quantity;
        }
        else if (txnType == "ADJUSTMENT")
        {
            item.CurrentStock = request.Quantity;
        }

        item.ModifiedAt = DateTimeOffset.UtcNow;
        item.ModifiedBy = createdBy;

        var txn = new StockTransaction
        {
            PropertyId = propertyId,
            TransactionNumber = txnNumber,
            InventoryItemId = item.Id,
            TransactionType = txnType,
            Quantity = request.Quantity,
            UnitCost = request.UnitCost > 0 ? request.UnitCost : item.UnitCost,
            TotalValue = request.Quantity * (request.UnitCost > 0 ? request.UnitCost : item.UnitCost),
            DepartmentCode = request.DepartmentCode,
            ReferenceNumber = request.ReferenceNumber,
            TransactionDate = DateTimeOffset.UtcNow,
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.StockTransactions.Add(txn);
        await _context.SaveChangesAsync(cancellationToken);

        return new StockTransactionDto(
            txn.Id,
            txn.TransactionNumber,
            txn.InventoryItemId,
            item.ItemCode,
            item.ItemName,
            txn.TransactionType,
            txn.Quantity,
            txn.UnitCost,
            txn.TotalValue,
            txn.DepartmentCode,
            txn.ReferenceNumber,
            txn.TransactionDate,
            txn.Notes
        );
    }

    public async Task<PagedResult<StockTransactionDto>> GetTransactionsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        long? itemId = null,
        string? transactionType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.StockTransactions
            .Include(t => t.InventoryItem)
            .AsNoTracking()
            .Where(t => t.PropertyId == propertyId && !t.IsDeleted);

        if (itemId.HasValue)
            query = query.Where(t => t.InventoryItemId == itemId.Value);

        if (!string.IsNullOrWhiteSpace(transactionType))
            query = query.Where(t => t.TransactionType == transactionType.Trim().ToUpperInvariant());

        var total = await query.CountAsync(cancellationToken);
        var txns = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new StockTransactionDto(
                t.Id,
                t.TransactionNumber,
                t.InventoryItemId,
                t.InventoryItem != null ? t.InventoryItem.ItemCode : string.Empty,
                t.InventoryItem != null ? t.InventoryItem.ItemName : string.Empty,
                t.TransactionType,
                t.Quantity,
                t.UnitCost,
                t.TotalValue,
                t.DepartmentCode,
                t.ReferenceNumber,
                t.TransactionDate,
                t.Notes
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<StockTransactionDto> { Items = txns, TotalCount = total, Page = page, PageSize = pageSize };
    }
}

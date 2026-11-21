using HotelOS.Contracts.Common;
using HotelOS.Contracts.Inventory;

namespace HotelOS.Application.Common.Interfaces;

public interface IInventoryService
{
    Task<PagedResult<InventoryItemDto>> GetItemsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? category = null,
        bool? lowStockOnly = null,
        CancellationToken cancellationToken = default);

    Task<InventoryItemDto> CreateItemAsync(
        CreateInventoryItemRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<InventoryItemDto> UpdateItemAsync(
        long id,
        UpdateInventoryItemRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    Task<StockTransactionDto> RecordTransactionAsync(
        RecordStockTransactionRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<PagedResult<StockTransactionDto>> GetTransactionsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        long? itemId = null,
        string? transactionType = null,
        CancellationToken cancellationToken = default);
}

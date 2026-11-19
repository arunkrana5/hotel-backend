using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ICurrentUserService _currentUser;

    public InventoryController(
        IInventoryService inventoryService,
        ICurrentUserService currentUser)
    {
        _inventoryService = inventoryService;
        _currentUser = currentUser;
    }

    [HttpGet("items")]
    public async Task<IActionResult> GetItems(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? lowStockOnly = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _inventoryService.GetItemsPagedAsync(propertyId, page, pageSize, category, search, lowStockOnly, cancellationToken);
        return Ok(ApiResponse<PagedResult<InventoryItemDto>>.Ok(result));
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem(
        [FromBody] CreateInventoryItemRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _inventoryService.CreateItemAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<InventoryItemDto>.Ok(result, "Inventory item created successfully."));
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] long? itemId = null,
        [FromQuery] string? transactionType = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _inventoryService.GetTransactionsPagedAsync(propertyId, page, pageSize, itemId, transactionType, cancellationToken);
        return Ok(ApiResponse<PagedResult<StockTransactionDto>>.Ok(result));
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> RecordTransaction(
        [FromBody] RecordStockTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _inventoryService.RecordTransactionAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<StockTransactionDto>.Ok(result, "Stock transaction posted successfully."));
    }
}

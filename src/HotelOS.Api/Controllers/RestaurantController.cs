using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Restaurant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/restaurant")]
[Authorize]
public class RestaurantController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    private readonly ICurrentUserService _currentUser;

    public RestaurantController(
        IRestaurantService restaurantService,
        ICurrentUserService currentUser)
    {
        _restaurantService = restaurantService;
        _currentUser = currentUser;
    }

    [HttpGet("tables")]
    public async Task<IActionResult> GetTables(
        [FromQuery] string? section = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _restaurantService.GetTablesAsync(propertyId, section, status, cancellationToken);
        return Ok(ApiResponse<List<DiningTableDto>>.Ok(result));
    }

    [HttpPut("tables/{id}/status")]
    public async Task<IActionResult> UpdateTableStatus(
        [FromRoute] long id,
        [FromBody] UpdateTableStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _restaurantService.UpdateTableStatusAsync(id, request, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Table #{id} not found.", 404));

        return Ok(ApiResponse<DiningTableDto>.Ok(result, "Table status updated."));
    }

    [HttpGet("menu")]
    public async Task<IActionResult> GetMenuItems(
        [FromQuery] string? category = null,
        [FromQuery] bool? isAvailableOnly = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _restaurantService.GetMenuItemsAsync(propertyId, category, isAvailableOnly, cancellationToken);
        return Ok(ApiResponse<List<MenuItemDto>>.Ok(result));
    }

    [HttpPost("menu")]
    public async Task<IActionResult> CreateMenuItem(
        [FromBody] CreateMenuItemRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _restaurantService.CreateMenuItemAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<MenuItemDto>.Ok(result, "Menu item created successfully."));
    }

    [HttpPut("menu/{id:long}")]
    public async Task<IActionResult> UpdateMenuItem(
        [FromRoute] long id,
        [FromBody] UpdateMenuItemRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _restaurantService.UpdateMenuItemAsync(id, request, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Menu item #{id} not found.", 404));

        return Ok(ApiResponse<MenuItemDto>.Ok(result, "Menu item updated successfully."));
    }

    [HttpPut("menu/{id}/availability")]
    public async Task<IActionResult> ToggleMenuItemAvailability(
        [FromRoute] long id,
        [FromBody] UpdateMenuItemAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _restaurantService.ToggleMenuItemAvailabilityAsync(id, request, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Menu item #{id} not found.", 404));

        return Ok(ApiResponse<MenuItemDto>.Ok(result, "Item availability updated."));
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? orderType = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _restaurantService.GetOrdersPagedAsync(
            propertyId, page, pageSize, status, orderType, cancellationToken);
        return Ok(ApiResponse<PagedResult<RestaurantOrderDto>>.Ok(result));
    }

    [HttpGet("kds")]
    public async Task<IActionResult> GetKdsTickets(CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _restaurantService.GetActiveKdsTicketsAsync(propertyId, cancellationToken);
        return Ok(ApiResponse<List<RestaurantOrderDto>>.Ok(result));
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateRestaurantOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        long? userId = _currentUser.UserId;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _restaurantService.CreateOrderAsync(request, propertyId, userId, user, cancellationToken);
        return Ok(ApiResponse<RestaurantOrderDto>.Ok(result, "Order created successfully."));
    }

    [HttpPut("orders/{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromRoute] long id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _restaurantService.UpdateOrderStatusAsync(id, request, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Order #{id} not found.", 404));

        return Ok(ApiResponse<RestaurantOrderDto>.Ok(result, "Order status updated."));
    }

    [HttpPut("orders/items/{itemId}/status")]
    public async Task<IActionResult> UpdateOrderItemStatus(
        [FromRoute] long itemId,
        [FromBody] UpdateOrderItemStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _restaurantService.UpdateOrderItemStatusAsync(itemId, request, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Order item #{itemId} not found.", 404));

        return Ok(ApiResponse<RestaurantOrderItemDto>.Ok(result, "Order item status updated."));
    }

    [HttpPost("orders/{id}/settle")]
    public async Task<IActionResult> SettleOrder(
        [FromRoute] long id,
        [FromBody] SettleRestaurantOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        try
        {
            var result = await _restaurantService.SettleOrderAsync(id, request, user, cancellationToken);
            if (result == null)
                return NotFound(ApiResponse.FailMessage($"Order #{id} not found.", 404));

            return Ok(ApiResponse<RestaurantOrderDto>.Ok(result, "Order settled successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }
}

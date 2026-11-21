using HotelOS.Contracts.Common;
using HotelOS.Contracts.Restaurant;

namespace HotelOS.Application.Common.Interfaces;

public interface IRestaurantService
{
    Task<List<DiningTableDto>> GetTablesAsync(
        long propertyId = 1,
        string? section = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<DiningTableDto?> UpdateTableStatusAsync(
        long tableId,
        UpdateTableStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<List<MenuItemDto>> GetMenuItemsAsync(
        long propertyId = 1,
        string? category = null,
        bool? isAvailableOnly = null,
        CancellationToken cancellationToken = default);

    Task<MenuItemDto> CreateMenuItemAsync(
        CreateMenuItemRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<MenuItemDto?> UpdateMenuItemAsync(
        long id,
        UpdateMenuItemRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<MenuItemDto?> ToggleMenuItemAvailabilityAsync(
        long menuItemId,
        UpdateMenuItemAvailabilityRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<PagedResult<RestaurantOrderDto>> GetOrdersPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? orderType = null,
        CancellationToken cancellationToken = default);

    Task<List<RestaurantOrderDto>> GetActiveKdsTicketsAsync(
        long propertyId = 1,
        CancellationToken cancellationToken = default);

    Task<RestaurantOrderDto> CreateOrderAsync(
        CreateRestaurantOrderRequest request,
        long propertyId,
        long? serverUserId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<RestaurantOrderDto?> UpdateOrderStatusAsync(
        long orderId,
        UpdateOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<RestaurantOrderItemDto?> UpdateOrderItemStatusAsync(
        long orderItemId,
        UpdateOrderItemStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<RestaurantOrderDto?> SettleOrderAsync(
        long orderId,
        SettleRestaurantOrderRequest request,
        string settledBy,
        CancellationToken cancellationToken = default);
}

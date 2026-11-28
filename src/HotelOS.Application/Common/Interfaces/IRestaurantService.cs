using HotelOS.Contracts.Common;
using HotelOS.Contracts.Restaurant;

namespace HotelOS.Application.Common.Interfaces;

public interface IRestaurantService
{
    // Menus
    Task<List<MenuDto>> GetMenusAsync(
        long propertyId = 1,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<MenuDto?> GetMenuByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<MenuDto> CreateMenuAsync(
        CreateMenuRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<MenuDto?> UpdateMenuAsync(
        long id,
        UpdateMenuRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<MenuDto?> ToggleMenuStatusAsync(
        long id,
        string status,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<MenuDto?> DuplicateMenuAsync(
        long id,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteMenuAsync(
        long id,
        string deletedBy,
        CancellationToken cancellationToken = default);

    // Categories
    Task<List<MenuCategoryDto>> GetCategoriesAsync(
        long propertyId = 1,
        long? menuId = null,
        CancellationToken cancellationToken = default);

    Task<MenuCategoryDto> CreateCategoryAsync(
        CreateMenuCategoryRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<MenuCategoryDto?> UpdateCategoryAsync(
        long id,
        UpdateMenuCategoryRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteCategoryAsync(
        long id,
        string deletedBy,
        CancellationToken cancellationToken = default);

    // Tables
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

    // Menu Items
    Task<List<MenuItemDto>> GetMenuItemsAsync(
        long propertyId = 1,
        string? category = null,
        bool? isAvailableOnly = null,
        long? menuId = null,
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

    Task<bool> DeleteMenuItemAsync(
        long id,
        string deletedBy,
        CancellationToken cancellationToken = default);

    // Orders
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

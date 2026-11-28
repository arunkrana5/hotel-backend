using HotelOS.Contracts.Common;
using HotelOS.Contracts.Operations;
using HotelOS.Contracts.Restaurant;

namespace HotelOS.Application.Common.Interfaces;

public interface IGuestOrderService
{
    // Staff operations
    Task<List<RoomQrCodeDto>> GetRoomQrCodesAsync(
        long propertyId = 1,
        CancellationToken cancellationToken = default);

    Task<RoomQrCodeDto> GenerateRoomQrAsync(
        long propertyId,
        GenerateRoomQrRequest request,
        long? userId,
        CancellationToken cancellationToken = default);

    Task<List<RoomQrCodeDto>> BulkGenerateRoomQrAsync(
        long propertyId,
        BulkGenerateRoomQrRequest request,
        long? userId,
        CancellationToken cancellationToken = default);

    Task<bool> RevokeRoomQrAsync(
        long qrCodeId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<RestaurantOrderDto>> GetRoomOrdersPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 15,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<RestaurantOrderDto?> UpdateRoomOrderStatusAsync(
        long orderId,
        StaffUpdateRoomOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    // Guest portal operations
    Task<QrValidationResultDto> ValidateQrTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<GuestSessionDto> InitializeSessionAsync(
        GuestSessionInitRequest request,
        CancellationToken cancellationToken = default);

    Task<List<MenuItemDto>> GetGuestMenuAsync(
        string sessionToken,
        string? category = null,
        CancellationToken cancellationToken = default);

    Task<GuestOrderSummaryDto> PlaceGuestOrderAsync(
        GuestPlaceOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<GuestOrderSummaryDto?> GetGuestOrderDetailsAsync(
        string orderNumber,
        CancellationToken cancellationToken = default);

    Task<GuestServiceRequestDto> RequestServiceAsync(
        GuestCreateServiceRequest request,
        CancellationToken cancellationToken = default);
}

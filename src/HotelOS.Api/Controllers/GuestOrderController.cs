using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Operations;
using HotelOS.Contracts.Restaurant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/guest-order")]
public class GuestOrderController : ControllerBase
{
    private readonly IGuestOrderService _guestOrderService;
    private readonly ICurrentUserService _currentUser;

    public GuestOrderController(
        IGuestOrderService guestOrderService,
        ICurrentUserService currentUser)
    {
        _guestOrderService = guestOrderService;
        _currentUser = currentUser;
    }

    // ==========================================
    // PUBLIC GUEST PORTAL ENDPOINTS (Anonymous)
    // ==========================================

    [AllowAnonymous]
    [HttpGet("resolve/{token}")]
    public async Task<IActionResult> ResolveToken(
        [FromRoute] string token,
        CancellationToken cancellationToken = default)
    {
        var result = await _guestOrderService.ValidateQrTokenAsync(token, cancellationToken);
        if (!result.IsValid)
        {
            return BadRequest(ApiResponse<QrValidationResultDto>.Fail(result.ErrorMessage ?? "Invalid QR token.", 400));
        }

        return Ok(ApiResponse<QrValidationResultDto>.Ok(result, "Room QR resolved successfully."));
    }

    [AllowAnonymous]
    [HttpPost("session")]
    public async Task<IActionResult> InitSession(
        [FromBody] GuestSessionInitRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _guestOrderService.InitializeSessionAsync(request, cancellationToken);
            return Ok(ApiResponse<GuestSessionDto>.Ok(session, "Session initialized."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.FailMessage(ex.Message, 401));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }

    [AllowAnonymous]
    [HttpGet("menu")]
    public async Task<IActionResult> GetGuestMenu(
        [FromQuery] string sessionToken,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _guestOrderService.GetGuestMenuAsync(sessionToken, category, cancellationToken);
            return Ok(ApiResponse<List<MenuItemDto>>.Ok(items));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.FailMessage(ex.Message, 401));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }

    [AllowAnonymous]
    [HttpPost("orders")]
    public async Task<IActionResult> PlaceGuestOrder(
        [FromBody] GuestPlaceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _guestOrderService.PlaceGuestOrderAsync(request, cancellationToken);
            return Ok(ApiResponse<GuestOrderSummaryDto>.Ok(summary, "Order placed successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.FailMessage(ex.Message, 401));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }

    [AllowAnonymous]
    [HttpGet("orders/{orderNumber}")]
    public async Task<IActionResult> GetOrderDetails(
        [FromRoute] string orderNumber,
        CancellationToken cancellationToken = default)
    {
        var order = await _guestOrderService.GetGuestOrderDetailsAsync(orderNumber, cancellationToken);
        if (order == null)
        {
            return NotFound(ApiResponse.FailMessage($"Order '{orderNumber}' not found.", 404));
        }

        return Ok(ApiResponse<GuestOrderSummaryDto>.Ok(order));
    }

    [AllowAnonymous]
    [HttpPost("services")]
    public async Task<IActionResult> RequestService(
        [FromBody] GuestCreateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var req = await _guestOrderService.RequestServiceAsync(request, cancellationToken);
            return Ok(ApiResponse<GuestServiceRequestDto>.Ok(req, "Service request submitted."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.FailMessage(ex.Message, 401));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }

    // ==========================================
    // PROTECTED STAFF MANAGEMENT ENDPOINTS
    // ==========================================

    [Authorize]
    [HttpGet("qr-codes")]
    public async Task<IActionResult> GetRoomQrCodes(
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var qrCodes = await _guestOrderService.GetRoomQrCodesAsync(propertyId, cancellationToken);
        return Ok(ApiResponse<List<RoomQrCodeDto>>.Ok(qrCodes));
    }

    [Authorize]
    [HttpPost("qr-codes/generate")]
    public async Task<IActionResult> GenerateRoomQr(
        [FromBody] GenerateRoomQrRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        long? userId = _currentUser.UserId;
        var result = await _guestOrderService.GenerateRoomQrAsync(propertyId, request, userId, cancellationToken);
        return Ok(ApiResponse<RoomQrCodeDto>.Ok(result, "Room QR code generated successfully."));
    }

    [Authorize]
    [HttpPost("qr-codes/bulk-generate")]
    public async Task<IActionResult> BulkGenerateRoomQr(
        [FromBody] BulkGenerateRoomQrRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        long? userId = _currentUser.UserId;
        var result = await _guestOrderService.BulkGenerateRoomQrAsync(propertyId, request, userId, cancellationToken);
        return Ok(ApiResponse<List<RoomQrCodeDto>>.Ok(result, $"Generated {result.Count} room QR codes."));
    }

    [Authorize]
    [HttpPost("qr-codes/{id}/revoke")]
    public async Task<IActionResult> RevokeRoomQr(
        [FromRoute] long id,
        CancellationToken cancellationToken = default)
    {
        var success = await _guestOrderService.RevokeRoomQrAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(ApiResponse.FailMessage($"QR code #{id} not found.", 404));
        }

        return Ok(ApiResponse<bool>.Ok(true, "Room QR code revoked."));
    }

    [Authorize]
    [HttpGet("room-orders")]
    public async Task<IActionResult> GetRoomOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _guestOrderService.GetRoomOrdersPagedAsync(propertyId, page, pageSize, status, cancellationToken);
        return Ok(ApiResponse<PagedResult<RestaurantOrderDto>>.Ok(result));
    }

    [Authorize]
    [HttpPut("room-orders/{id}/status")]
    public async Task<IActionResult> UpdateRoomOrderStatus(
        [FromRoute] long id,
        [FromBody] StaffUpdateRoomOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "STAFF";
        var result = await _guestOrderService.UpdateRoomOrderStatusAsync(id, request, user, cancellationToken);
        if (result == null)
        {
            return NotFound(ApiResponse.FailMessage($"Room order #{id} not found.", 404));
        }

        return Ok(ApiResponse<RestaurantOrderDto>.Ok(result, "Order status updated."));
    }
}

using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Banquet;
using HotelOS.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/banquet")]
[Authorize]
public class BanquetController : ControllerBase
{
    private readonly IBanquetService _banquetService;
    private readonly ICurrentUserService _currentUser;

    public BanquetController(
        IBanquetService banquetService,
        ICurrentUserService currentUser)
    {
        _banquetService = banquetService;
        _currentUser = currentUser;
    }

    [HttpGet("venues")]
    public async Task<IActionResult> GetVenues(CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _banquetService.GetVenuesAsync(propertyId, cancellationToken);
        return Ok(ApiResponse<List<BanquetVenueDto>>.Ok(result));
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] DateTimeOffset? fromDate = null,
        [FromQuery] DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _banquetService.GetBookingsPagedAsync(
            propertyId, page, pageSize, status, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<PagedResult<BanquetBookingDto>>.Ok(result));
    }

    [HttpPost("bookings")]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBanquetBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _banquetService.CreateBookingAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<BanquetBookingDto>.Ok(result, "Banquet booking confirmed successfully."));
    }

    [HttpPut("bookings/{id}/status")]
    public async Task<IActionResult> UpdateBookingStatus(
        [FromRoute] long id,
        [FromBody] UpdateBanquetBookingStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _banquetService.UpdateBookingStatusAsync(id, request, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Booking #{id} not found.", 404));

        return Ok(ApiResponse<BanquetBookingDto>.Ok(result, "Booking status updated."));
    }
}

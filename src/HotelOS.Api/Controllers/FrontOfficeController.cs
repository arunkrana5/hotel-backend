using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Pms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/pms/front-office")]
[Authorize]
public class FrontOfficeController : ControllerBase
{
    private readonly IPmsService _pmsService;
    private readonly ICurrentUserService _currentUser;

    public FrontOfficeController(IPmsService pmsService, ICurrentUserService currentUser)
    {
        _pmsService = pmsService;
        _currentUser = currentUser;
    }

    [HttpGet("room-grid")]
    public async Task<IActionResult> GetRoomGrid(
        [FromQuery] int? floor = null,
        [FromQuery] string? status = null,
        [FromQuery] string? roomType = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _pmsService.GetRoomGridAsync(propertyId, floor, status, roomType, cancellationToken);
        return Ok(ApiResponse<RoomGridSummaryDto>.Ok(result));
    }

    [HttpGet("arrivals")]
    public async Task<IActionResult> GetArrivals(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string todayStr = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
        var result = await _pmsService.GetReservationsPagedAsync(propertyId, page, pageSize, search, "CONF", todayStr, null, cancellationToken);
        return Ok(ApiResponse<PagedResult<ReservationDto>>.Ok(result));
    }

    [HttpGet("departures")]
    public async Task<IActionResult> GetDepartures(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _pmsService.GetReservationsPagedAsync(propertyId, page, pageSize, search, "CHKI", null, null, cancellationToken);
        return Ok(ApiResponse<PagedResult<ReservationDto>>.Ok(result));
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn(
        [FromBody] CheckInRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string user = _currentUser.Username ?? "SYSTEM";
            var result = await _pmsService.CheckInGuestAsync(request, user, cancellationToken);
            return Ok(ApiResponse<ReservationDto>.Ok(result, "Guest checked in successfully. Room allocated and Folio activated."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut(
        [FromBody] CheckOutRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string user = _currentUser.Username ?? "SYSTEM";
            var result = await _pmsService.CheckOutGuestAsync(request, user, cancellationToken);
            return Ok(ApiResponse<ReservationDto>.Ok(result, "Guest checked out successfully. Folio closed and room marked Dirty for housekeeping."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }
}

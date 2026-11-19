using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Pms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/pms/reservations")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IPmsService _pmsService;
    private readonly ICurrentUserService _currentUser;

    public ReservationController(IPmsService pmsService, ICurrentUserService currentUser)
    {
        _pmsService = pmsService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetReservations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? checkInDate = null,
        [FromQuery] string? checkOutDate = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _pmsService.GetReservationsPagedAsync(
            propertyId,
            page,
            pageSize,
            search,
            status,
            checkInDate,
            checkOutDate,
            cancellationToken);

        return Ok(ApiResponse<PagedResult<ReservationDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservationById(
        [FromRoute] long id,
        CancellationToken cancellationToken = default)
    {
        var result = await _pmsService.GetReservationByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Reservation #{id} not found.", 404));

        return Ok(ApiResponse<ReservationDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation(
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            long propertyId = _currentUser.PropertyId ?? 1;
            string user = _currentUser.Username ?? "SYSTEM";
            var result = await _pmsService.CreateReservationAsync(request, propertyId, user, cancellationToken);
            return Ok(ApiResponse<ReservationDto>.Ok(result, $"Reservation {result.ConfirmationNumber} created successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }
}

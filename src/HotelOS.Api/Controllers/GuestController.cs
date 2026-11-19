using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Pms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/pms/guests")]
[Authorize]
public class GuestController : ControllerBase
{
    private readonly IPmsService _pmsService;
    private readonly ICurrentUserService _currentUser;

    public GuestController(IPmsService pmsService, ICurrentUserService currentUser)
    {
        _pmsService = pmsService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetGuests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? vipStatus = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _pmsService.GetGuestsPagedAsync(propertyId, page, pageSize, search, vipStatus, cancellationToken);
        return Ok(ApiResponse<PagedResult<GuestDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGuestById([FromRoute] long id, CancellationToken cancellationToken = default)
    {
        var result = await _pmsService.GetGuestByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Guest #{id} not found.", 404));

        return Ok(ApiResponse<GuestDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateGuest(
        [FromBody] CreateGuestRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            long propertyId = _currentUser.PropertyId ?? 1;
            string user = _currentUser.Username ?? "SYSTEM";
            var result = await _pmsService.CreateGuestAsync(request, propertyId, user, cancellationToken);
            return Ok(ApiResponse<GuestDto>.Ok(result, "Guest profile created successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }
}

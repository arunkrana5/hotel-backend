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
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _pmsService.GetGuestsPagedAsync(propertyId, page, pageSize, search, vipStatus, isActive, cancellationToken);
        return Ok(ApiResponse<PagedResult<GuestDto>>.Ok(result));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetGuestStats(CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var stats = await _pmsService.GetGuestCrmStatsAsync(propertyId, cancellationToken);
        return Ok(ApiResponse<GuestCrmStatsDto>.Ok(stats));
    }

    [HttpGet("check-duplicate")]
    public async Task<IActionResult> CheckDuplicate(
        [FromQuery] string email,
        [FromQuery] string phone,
        [FromQuery] long? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _pmsService.CheckDuplicateGuestAsync(email, phone, excludeId, propertyId, cancellationToken);
        return Ok(ApiResponse<DuplicateCheckResult>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGuestById([FromRoute] long id, CancellationToken cancellationToken = default)
    {
        var result = await _pmsService.GetGuestByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Guest #{id} not found.", 404));

        return Ok(ApiResponse<GuestDto>.Ok(result));
    }

    [HttpGet("{id}/profile")]
    public async Task<IActionResult> GetGuestProfile([FromRoute] long id, CancellationToken cancellationToken = default)
    {
        var profile = await _pmsService.GetGuestCrmProfileAsync(id, cancellationToken);
        if (profile == null)
            return NotFound(ApiResponse.FailMessage($"Guest #{id} not found.", 404));

        return Ok(ApiResponse<GuestCrmProfileDto>.Ok(profile));
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGuest(
        [FromRoute] long id,
        [FromBody] UpdateGuestRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string user = _currentUser.Username ?? "SYSTEM";
            var result = await _pmsService.UpdateGuestAsync(id, request, user, cancellationToken);
            return Ok(ApiResponse<GuestDto>.Ok(result, "Guest profile updated successfully."));
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(ApiResponse.FailMessage(knf.Message, 404));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> SetGuestStatus(
        [FromRoute] long id,
        [FromBody] SetGuestStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        bool success = await _pmsService.SetGuestStatusAsync(id, request.IsActive, user, cancellationToken);
        if (!success)
            return NotFound(ApiResponse.FailMessage($"Guest #{id} not found.", 404));

        string msg = request.IsActive ? "Guest profile activated successfully." : "Guest profile deactivated.";
        return Ok(ApiResponse.OkMessage(msg));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGuest(
        [FromRoute] long id,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var (success, message) = await _pmsService.DeleteGuestAsync(id, user, cancellationToken);
        if (!success)
        {
            return BadRequest(ApiResponse.FailMessage(message, 400));
        }

        return Ok(ApiResponse.OkMessage(message));
    }
}

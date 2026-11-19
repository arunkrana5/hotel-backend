using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Pms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/pms/rooms")]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IPmsService _pmsService;
    private readonly ICurrentUserService _currentUser;

    public RoomController(IPmsService pmsService, ICurrentUserService currentUser)
    {
        _pmsService = pmsService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetRooms(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? roomType = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _pmsService.GetRoomsPagedAsync(propertyId, page, pageSize, search, status, roomType, cancellationToken);
        return Ok(ApiResponse<PagedResult<RoomDto>>.Ok(result));
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateRoomStatus(
        [FromRoute] long id,
        [FromBody] UpdateRoomStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _pmsService.UpdateRoomStatusAsync(id, request.StatusCode, request.Notes, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Room #{id} not found.", 404));

        return Ok(ApiResponse<RoomDto>.Ok(result, "Room status updated successfully."));
    }
}

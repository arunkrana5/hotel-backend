using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/operations")]
[Authorize]
public class OperationsController : ControllerBase
{
    private readonly IOperationsService _operationsService;
    private readonly ICurrentUserService _currentUser;

    public OperationsController(
        IOperationsService operationsService,
        ICurrentUserService currentUser)
    {
        _operationsService = operationsService;
        _currentUser = currentUser;
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? department = null,
        [FromQuery] string? priority = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _operationsService.GetGuestRequestsPagedAsync(
            propertyId, page, pageSize, status, department, priority, cancellationToken);
        return Ok(ApiResponse<PagedResult<GuestServiceRequestDto>>.Ok(result));
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest(
        [FromBody] CreateGuestServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _operationsService.CreateGuestRequestAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<GuestServiceRequestDto>.Ok(result, "Guest service request created."));
    }

    [HttpPut("requests/{id}/status")]
    public async Task<IActionResult> UpdateRequestStatus(
        [FromRoute] long id,
        [FromBody] UpdateGuestServiceRequestStatus request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _operationsService.UpdateGuestRequestStatusAsync(id, request, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Service request #{id} not found.", 404));

        return Ok(ApiResponse<GuestServiceRequestDto>.Ok(result, "Service request updated."));
    }
}

using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Pms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/pms/folios")]
[Authorize]
public class FolioController : ControllerBase
{
    private readonly IPmsService _pmsService;
    private readonly ICurrentUserService _currentUser;

    public FolioController(IPmsService pmsService, ICurrentUserService currentUser)
    {
        _pmsService = pmsService;
        _currentUser = currentUser;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFolioById([FromRoute] long id, CancellationToken cancellationToken = default)
    {
        var result = await _pmsService.GetFolioByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Folio #{id} not found.", 404));

        return Ok(ApiResponse<FolioDto>.Ok(result));
    }

    [HttpGet("by-reservation/{reservationId}")]
    public async Task<IActionResult> GetFolioByReservationId([FromRoute] long reservationId, CancellationToken cancellationToken = default)
    {
        var result = await _pmsService.GetFolioByReservationIdAsync(reservationId, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Folio for reservation #{reservationId} not found.", 404));

        return Ok(ApiResponse<FolioDto>.Ok(result));
    }

    [HttpPost("charges")]
    public async Task<IActionResult> PostCharge([FromBody] PostChargeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            string user = _currentUser.Username ?? "SYSTEM";
            var result = await _pmsService.PostChargeAsync(request, user, cancellationToken);
            return Ok(ApiResponse<FolioDto>.Ok(result, "Charge posted to folio successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }

    [HttpPost("payments")]
    public async Task<IActionResult> PostPayment([FromBody] PostPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            string user = _currentUser.Username ?? "SYSTEM";
            var result = await _pmsService.PostPaymentAsync(request, user, cancellationToken);
            return Ok(ApiResponse<FolioDto>.Ok(result, "Payment posted to folio successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }
}

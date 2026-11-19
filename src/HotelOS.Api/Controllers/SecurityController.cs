using System.Security.Claims;
using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/security")]
[Authorize]
public class SecurityController : ControllerBase
{
    private readonly ISecurityService _securityService;
    private readonly ICurrentUserService _currentUser;

    public SecurityController(
        ISecurityService securityService,
        ICurrentUserService currentUser)
    {
        _securityService = securityService;
        _currentUser = currentUser;
    }

    [HttpGet("visitors")]
    public async Task<IActionResult> GetVisitors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _securityService.GetVisitorsPagedAsync(propertyId, page, pageSize, status, search, cancellationToken);
        return Ok(ApiResponse<PagedResult<VisitorLogDto>>.Ok(result));
    }

    [HttpPost("visitors/check-in")]
    public async Task<IActionResult> CheckInVisitor(
        [FromBody] CheckInVisitorRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _securityService.CheckInVisitorAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<VisitorLogDto>.Ok(result, "Visitor checked in successfully."));
    }

    [HttpPut("visitors/{id}/check-out")]
    public async Task<IActionResult> CheckOutVisitor(
        [FromRoute] long id,
        [FromBody] CheckOutVisitorRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _securityService.CheckOutVisitorAsync(id, request, user, cancellationToken);
        if (result == null) return NotFound(ApiResponse<VisitorLogDto>.Fail("Visitor log not found."));
        return Ok(ApiResponse<VisitorLogDto>.Ok(result, "Visitor checked out successfully."));
    }

    [HttpGet("incidents")]
    public async Task<IActionResult> GetIncidents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? severity = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _securityService.GetIncidentsPagedAsync(propertyId, page, pageSize, severity, status, cancellationToken);
        return Ok(ApiResponse<PagedResult<IncidentReportDto>>.Ok(result));
    }

    [HttpPost("incidents")]
    public async Task<IActionResult> ReportIncident(
        [FromBody] CreateIncidentReportRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        long? userId = _currentUser.UserId;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _securityService.ReportIncidentAsync(request, propertyId, userId, user, cancellationToken);
        return Ok(ApiResponse<IncidentReportDto>.Ok(result, "Incident report submitted successfully."));
    }

    [HttpPut("incidents/{id}/status")]
    public async Task<IActionResult> UpdateIncidentStatus(
        [FromRoute] long id,
        [FromBody] UpdateIncidentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _securityService.UpdateIncidentStatusAsync(id, request, user, cancellationToken);
        if (result == null) return NotFound(ApiResponse<IncidentReportDto>.Fail("Incident report not found."));
        return Ok(ApiResponse<IncidentReportDto>.Ok(result, "Incident status updated."));
    }
}

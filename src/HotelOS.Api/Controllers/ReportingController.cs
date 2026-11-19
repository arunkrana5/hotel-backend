using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportingController : ControllerBase
{
    private readonly IReportingService _reportingService;
    private readonly ICurrentUserService _currentUserService;

    public ReportingController(IReportingService reportingService, ICurrentUserService currentUserService)
    {
        _reportingService = reportingService;
        _currentUserService = currentUserService;
    }

    private long GetEffectivePropertyId(long? requestedPropertyId)
    {
        if (requestedPropertyId.HasValue && requestedPropertyId.Value > 0)
            return requestedPropertyId.Value;

        return _currentUserService.PropertyId ?? 1;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<ExecutiveDashboardDto>>> GetExecutiveDashboard([FromQuery] long? propertyId, CancellationToken ct)
    {
        var targetPropertyId = GetEffectivePropertyId(propertyId);
        var result = await _reportingService.GetExecutiveDashboardAsync(targetPropertyId, ct);
        return Ok(ApiResponse<ExecutiveDashboardDto>.Ok(result));
    }

    [HttpGet("night-audit")]
    public async Task<ActionResult<ApiResponse<NightAuditReportDto>>> GetNightAuditReport(
        [FromQuery] long? propertyId,
        [FromQuery] DateOnly? auditDate,
        CancellationToken ct)
    {
        var targetPropertyId = GetEffectivePropertyId(propertyId);
        var result = await _reportingService.GetNightAuditReportAsync(targetPropertyId, auditDate, ct);
        return Ok(ApiResponse<NightAuditReportDto>.Ok(result));
    }

    [HttpGet("revenue-ledger")]
    public async Task<ActionResult<ApiResponse<RevenueTrialBalanceReportDto>>> GetRevenueTrialBalance(
        [FromQuery] long? propertyId,
        CancellationToken ct)
    {
        var targetPropertyId = GetEffectivePropertyId(propertyId);
        var result = await _reportingService.GetRevenueTrialBalanceReportAsync(targetPropertyId, ct);
        return Ok(ApiResponse<RevenueTrialBalanceReportDto>.Ok(result));
    }

    [HttpGet("occupancy-forecast")]
    public async Task<ActionResult<ApiResponse<OccupancyForecastReportDto>>> GetOccupancyForecast(
        [FromQuery] long? propertyId,
        [FromQuery] int days = 14,
        CancellationToken ct = default)
    {
        var targetPropertyId = GetEffectivePropertyId(propertyId);
        var result = await _reportingService.GetOccupancyForecastReportAsync(targetPropertyId, Math.Clamp(days, 7, 60), ct);
        return Ok(ApiResponse<OccupancyForecastReportDto>.Ok(result));
    }

    [HttpGet("procurement-spend")]
    public async Task<ActionResult<ApiResponse<DepartmentProcurementReportDto>>> GetProcurementSpend(
        [FromQuery] long? propertyId,
        CancellationToken ct)
    {
        var targetPropertyId = GetEffectivePropertyId(propertyId);
        var result = await _reportingService.GetDepartmentProcurementReportAsync(targetPropertyId, ct);
        return Ok(ApiResponse<DepartmentProcurementReportDto>.Ok(result));
    }

    [HttpGet("payroll-summary")]
    public async Task<ActionResult<ApiResponse<StaffPayrollSummaryReportDto>>> GetPayrollSummary(
        [FromQuery] long? propertyId,
        CancellationToken ct)
    {
        var targetPropertyId = GetEffectivePropertyId(propertyId);
        var result = await _reportingService.GetStaffPayrollReportAsync(targetPropertyId, ct);
        return Ok(ApiResponse<StaffPayrollSummaryReportDto>.Ok(result));
    }

    [HttpGet("export/{reportCode}")]
    public async Task<IActionResult> ExportReportCsv(
        string reportCode,
        [FromQuery] long? propertyId,
        CancellationToken ct)
    {
        var targetPropertyId = GetEffectivePropertyId(propertyId);
        var csvBytes = await _reportingService.ExportReportCsvAsync(reportCode, targetPropertyId, ct);
        var filename = $"{reportCode.ToLowerInvariant()}_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(csvBytes, "text/csv", filename);
    }
}

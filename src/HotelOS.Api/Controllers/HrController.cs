using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Hr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/hr")]
[Authorize]
public class HrController : ControllerBase
{
    private readonly IHrService _hrService;
    private readonly ICurrentUserService _currentUser;

    public HrController(
        IHrService hrService,
        ICurrentUserService currentUser)
    {
        _hrService = hrService;
        _currentUser = currentUser;
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? department = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _hrService.GetEmployeesPagedAsync(propertyId, page, pageSize, department, status, search, cancellationToken);
        return Ok(ApiResponse<PagedResult<EmployeeDto>>.Ok(result));
    }

    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployee(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _hrService.CreateEmployeeAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<EmployeeDto>.Ok(result, "Employee enrolled successfully."));
    }

    [HttpPut("employees/{id}/status")]
    public async Task<IActionResult> UpdateEmployeeStatus(
        [FromRoute] long id,
        [FromBody] UpdateEmployeeStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _hrService.UpdateEmployeeStatusAsync(id, request, user, cancellationToken);
        if (result == null) return NotFound(ApiResponse<EmployeeDto>.Fail("Employee not found."));
        return Ok(ApiResponse<EmployeeDto>.Ok(result, "Employee status updated."));
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendance(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? date = null,
        [FromQuery] string? department = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _hrService.GetAttendancePagedAsync(propertyId, page, pageSize, date, department, cancellationToken);
        return Ok(ApiResponse<PagedResult<AttendanceRecordDto>>.Ok(result));
    }

    [HttpPost("attendance")]
    public async Task<IActionResult> RecordAttendance(
        [FromBody] RecordAttendanceRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _hrService.RecordAttendanceAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<AttendanceRecordDto>.Ok(result, "Attendance recorded successfully."));
    }
}

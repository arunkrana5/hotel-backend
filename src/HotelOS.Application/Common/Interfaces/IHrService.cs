using HotelOS.Contracts.Common;
using HotelOS.Contracts.Hr;

namespace HotelOS.Application.Common.Interfaces;

public interface IHrService
{
    // Employees
    Task<PagedResult<EmployeeDto>> GetEmployeesPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? department = null,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<EmployeeDto> CreateEmployeeAsync(
        CreateEmployeeRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<EmployeeDto?> UpdateEmployeeStatusAsync(
        long employeeId,
        UpdateEmployeeStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<EmployeeDto?> UpdateEmployeeAsync(
        long employeeId,
        UpdateEmployeeRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    // Attendance
    Task<PagedResult<AttendanceRecordDto>> GetAttendancePagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? date = null,
        string? department = null,
        CancellationToken cancellationToken = default);

    Task<AttendanceRecordDto> RecordAttendanceAsync(
        RecordAttendanceRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);
}

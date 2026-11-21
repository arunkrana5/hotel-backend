using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Hr;
using HotelOS.Domain.Hr.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class HrService : IHrService
{
    private readonly HotelOSDbContext _context;

    public HrService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<EmployeeDto>> GetEmployeesPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? department = null,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .AsNoTracking()
            .Where(e => e.PropertyId == propertyId && !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(e => e.DepartmentCode == department.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(e => e.Status == status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(e =>
                e.EmployeeCode.ToLower().Contains(s) ||
                e.FirstName.ToLower().Contains(s) ||
                e.LastName.ToLower().Contains(s) ||
                e.Designation.ToLower().Contains(s) ||
                e.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.EmployeeCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeDto(
                e.Id,
                e.EmployeeCode,
                e.UserId,
                e.FirstName,
                e.LastName,
                (e.FirstName + " " + e.LastName).Trim(),
                e.DepartmentCode,
                e.Designation,
                e.DateOfJoining.ToString("yyyy-MM-dd"),
                e.EmploymentType,
                e.BasicSalary,
                e.Status,
                e.Email,
                e.Phone,
                e.EmergencyContact
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<EmployeeDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(
        CreateEmployeeRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var dateOfJoining = DateOnly.TryParse(request.DateOfJoining, out var doj)
            ? doj
            : DateOnly.FromDateTime(DateTime.UtcNow);

        var employee = new Employee
        {
            PropertyId = propertyId,
            EmployeeCode = request.EmployeeCode.Trim().ToUpperInvariant(),
            UserId = request.UserId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DepartmentCode = request.DepartmentCode.Trim().ToUpperInvariant(),
            Designation = request.Designation.Trim(),
            DateOfJoining = dateOfJoining,
            EmploymentType = request.EmploymentType.Trim().ToUpperInvariant(),
            BasicSalary = request.BasicSalary,
            Status = "ACTIVE",
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone.Trim(),
            EmergencyContact = request.EmergencyContact?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        return new EmployeeDto(
            employee.Id,
            employee.EmployeeCode,
            employee.UserId,
            employee.FirstName,
            employee.LastName,
            $"{employee.FirstName} {employee.LastName}".Trim(),
            employee.DepartmentCode,
            employee.Designation,
            employee.DateOfJoining.ToString("yyyy-MM-dd"),
            employee.EmploymentType,
            employee.BasicSalary,
            employee.Status,
            employee.Email,
            employee.Phone,
            employee.EmergencyContact
        );
    }

    public async Task<EmployeeDto?> UpdateEmployeeAsync(
        long employeeId,
        UpdateEmployeeRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted, cancellationToken);

        if (employee == null) return null;

        if (!string.IsNullOrWhiteSpace(request.FirstName)) employee.FirstName = request.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(request.LastName)) employee.LastName = request.LastName.Trim();
        if (!string.IsNullOrWhiteSpace(request.DepartmentCode)) employee.DepartmentCode = request.DepartmentCode.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(request.Designation)) employee.Designation = request.Designation.Trim();
        if (!string.IsNullOrWhiteSpace(request.EmploymentType)) employee.EmploymentType = request.EmploymentType.Trim().ToUpperInvariant();
        if (request.BasicSalary > 0) employee.BasicSalary = request.BasicSalary;
        if (!string.IsNullOrWhiteSpace(request.Email)) employee.Email = request.Email.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(request.Phone)) employee.Phone = request.Phone.Trim();
        employee.EmergencyContact = request.EmergencyContact?.Trim();

        employee.ModifiedAt = DateTimeOffset.UtcNow;
        employee.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new EmployeeDto(
            employee.Id,
            employee.EmployeeCode,
            employee.UserId,
            employee.FirstName,
            employee.LastName,
            $"{employee.FirstName} {employee.LastName}".Trim(),
            employee.DepartmentCode,
            employee.Designation,
            employee.DateOfJoining.ToString("yyyy-MM-dd"),
            employee.EmploymentType,
            employee.BasicSalary,
            employee.Status,
            employee.Email,
            employee.Phone,
            employee.EmergencyContact
        );
    }

    public async Task<EmployeeDto?> UpdateEmployeeStatusAsync(
        long employeeId,
        UpdateEmployeeStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted, cancellationToken);

        if (employee == null) return null;

        employee.Status = request.Status.Trim().ToUpperInvariant();
        employee.ModifiedAt = DateTimeOffset.UtcNow;
        employee.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new EmployeeDto(
            employee.Id,
            employee.EmployeeCode,
            employee.UserId,
            employee.FirstName,
            employee.LastName,
            $"{employee.FirstName} {employee.LastName}".Trim(),
            employee.DepartmentCode,
            employee.Designation,
            employee.DateOfJoining.ToString("yyyy-MM-dd"),
            employee.EmploymentType,
            employee.BasicSalary,
            employee.Status,
            employee.Email,
            employee.Phone,
            employee.EmergencyContact
        );
    }

    public async Task<PagedResult<AttendanceRecordDto>> GetAttendancePagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? date = null,
        string? department = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
            .AsNoTracking()
            .Where(a => a.PropertyId == propertyId && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var filterDate))
        {
            query = query.Where(a => a.Date == filterDate);
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            var dept = department.Trim().ToUpperInvariant();
            query = query.Where(a => a.Employee != null && a.Employee.DepartmentCode == dept);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Employee!.EmployeeCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AttendanceRecordDto(
                a.Id,
                a.EmployeeId,
                a.Employee != null ? a.Employee.EmployeeCode : string.Empty,
                a.Employee != null ? (a.Employee.FirstName + " " + a.Employee.LastName).Trim() : string.Empty,
                a.Employee != null ? a.Employee.DepartmentCode : string.Empty,
                a.Date.ToString("yyyy-MM-dd"),
                a.CheckInTime != null ? a.CheckInTime.Value.ToString("HH:mm") : null,
                a.CheckOutTime != null ? a.CheckOutTime.Value.ToString("HH:mm") : null,
                a.Shift,
                a.Status,
                a.OvertimeHours,
                a.Remarks
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<AttendanceRecordDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AttendanceRecordDto> RecordAttendanceAsync(
        RecordAttendanceRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var recordDate = DateOnly.TryParse(request.Date, out var rd)
            ? rd
            : DateOnly.FromDateTime(DateTime.UtcNow);

        TimeOnly? checkIn = null;
        if (!string.IsNullOrWhiteSpace(request.CheckInTime) && TimeOnly.TryParse(request.CheckInTime, out var cin))
            checkIn = cin;

        TimeOnly? checkOut = null;
        if (!string.IsNullOrWhiteSpace(request.CheckOutTime) && TimeOnly.TryParse(request.CheckOutTime, out var cout))
            checkOut = cout;

        var existing = await _context.AttendanceRecords
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.PropertyId == propertyId && a.EmployeeId == request.EmployeeId && a.Date == recordDate && !a.IsDeleted, cancellationToken);

        if (existing != null)
        {
            existing.CheckInTime = checkIn ?? existing.CheckInTime;
            existing.CheckOutTime = checkOut ?? existing.CheckOutTime;
            existing.Shift = request.Shift.Trim().ToUpperInvariant();
            existing.Status = request.Status.Trim().ToUpperInvariant();
            existing.OvertimeHours = request.OvertimeHours;
            existing.Remarks = request.Remarks?.Trim();
            existing.ModifiedAt = DateTimeOffset.UtcNow;
            existing.ModifiedBy = createdBy;

            await _context.SaveChangesAsync(cancellationToken);

            var emp = existing.Employee ?? await _context.Employees.FirstOrDefaultAsync(e => e.Id == existing.EmployeeId, cancellationToken);

            return new AttendanceRecordDto(
                existing.Id,
                existing.EmployeeId,
                emp?.EmployeeCode ?? string.Empty,
                emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : string.Empty,
                emp?.DepartmentCode ?? string.Empty,
                existing.Date.ToString("yyyy-MM-dd"),
                existing.CheckInTime?.ToString("HH:mm"),
                existing.CheckOutTime?.ToString("HH:mm"),
                existing.Shift,
                existing.Status,
                existing.OvertimeHours,
                existing.Remarks
            );
        }

        var record = new AttendanceRecord
        {
            PropertyId = propertyId,
            EmployeeId = request.EmployeeId,
            Date = recordDate,
            CheckInTime = checkIn,
            CheckOutTime = checkOut,
            Shift = request.Shift.Trim().ToUpperInvariant(),
            Status = request.Status.Trim().ToUpperInvariant(),
            OvertimeHours = request.OvertimeHours,
            Remarks = request.Remarks?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.AttendanceRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        return new AttendanceRecordDto(
            record.Id,
            record.EmployeeId,
            employee?.EmployeeCode ?? string.Empty,
            employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : string.Empty,
            employee?.DepartmentCode ?? string.Empty,
            record.Date.ToString("yyyy-MM-dd"),
            record.CheckInTime?.ToString("HH:mm"),
            record.CheckOutTime?.ToString("HH:mm"),
            record.Shift,
            record.Status,
            record.OvertimeHours,
            record.Remarks
        );
    }
}

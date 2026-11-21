namespace HotelOS.Contracts.Hr;

public record EmployeeDto(
    long Id,
    string EmployeeCode,
    long? UserId,
    string FirstName,
    string LastName,
    string FullName,
    string DepartmentCode,
    string Designation,
    string DateOfJoining,
    string EmploymentType,
    decimal BasicSalary,
    string Status,
    string Email,
    string Phone,
    string? EmergencyContact
);

public record CreateEmployeeRequest(
    string EmployeeCode,
    long? UserId,
    string FirstName,
    string LastName,
    string DepartmentCode,
    string Designation,
    string DateOfJoining,
    string EmploymentType,
    decimal BasicSalary,
    string Email,
    string Phone,
    string? EmergencyContact
);

public record UpdateEmployeeStatusRequest(
    string Status
);

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string DepartmentCode,
    string Designation,
    string EmploymentType,
    decimal BasicSalary,
    string Email,
    string Phone,
    string? EmergencyContact
);

public record AttendanceRecordDto(
    long Id,
    long EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string DepartmentCode,
    string Date,
    string? CheckInTime,
    string? CheckOutTime,
    string Shift,
    string Status,
    decimal OvertimeHours,
    string? Remarks
);

public record RecordAttendanceRequest(
    long EmployeeId,
    string Date,
    string? CheckInTime,
    string? CheckOutTime,
    string Shift,
    string Status,
    decimal OvertimeHours,
    string? Remarks
);

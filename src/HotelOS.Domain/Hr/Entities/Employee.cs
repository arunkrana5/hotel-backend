namespace HotelOS.Domain.Hr.Entities;

using HotelOS.Domain.Common;
using HotelOS.Domain.Identity.Entities;

public class Employee : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string EmployeeCode { get; set; } = string.Empty;
    public long? UserId { get; set; }
    public User? User { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = "FO";
    public string Designation { get; set; } = string.Empty;
    public DateOnly DateOfJoining { get; set; }
    public string EmploymentType { get; set; } = "FULL_TIME"; // FULL_TIME, PART_TIME, CONTRACT
    public decimal BasicSalary { get; set; }
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, ON_LEAVE, TERMINATED
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? EmergencyContact { get; set; }
}

public class AttendanceRecord : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public long EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public string Shift { get; set; } = "MORNING"; // MORNING, EVENING, NIGHT, GENERAL
    public string Status { get; set; } = "PRESENT"; // PRESENT, ABSENT, LATE, HALF_DAY, ON_LEAVE
    public decimal OvertimeHours { get; set; } = 0;
    public string? Remarks { get; set; }
}

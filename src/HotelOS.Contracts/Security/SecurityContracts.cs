namespace HotelOS.Contracts.Security;

public record VisitorLogDto(
    long Id,
    string PassNumber,
    string VisitorName,
    string IdType,
    string IdNumber,
    string Phone,
    string Purpose,
    string VisitingDepartment,
    string? RoomOrArea,
    DateTimeOffset CheckInTime,
    DateTimeOffset? CheckOutTime,
    string Status,
    string? BadgeNumber
);

public record CheckInVisitorRequest(
    string VisitorName,
    string IdType,
    string IdNumber,
    string Phone,
    string Purpose,
    string VisitingDepartment,
    string? RoomOrArea,
    string? BadgeNumber
);

public record CheckOutVisitorRequest(
    string? Remarks
);

public record IncidentReportDto(
    long Id,
    string IncidentNumber,
    DateTimeOffset IncidentDate,
    string Location,
    string Severity,
    string Category,
    string Description,
    long? ReportedByUserId,
    string? ReportedByUserName,
    string Status,
    string? ActionTaken,
    DateTimeOffset? ResolvedAt
);

public record CreateIncidentReportRequest(
    DateTimeOffset IncidentDate,
    string Location,
    string Severity,
    string Category,
    string Description,
    string? ActionTaken
);

public record UpdateIncidentStatusRequest(
    string Status,
    string? ActionTaken
);

using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Security;
using HotelOS.Domain.Security.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class SecurityService : ISecurityService
{
    private readonly HotelOSDbContext _context;

    public SecurityService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<VisitorLogDto>> GetVisitorsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.VisitorLogs
            .AsNoTracking()
            .Where(v => v.PropertyId == propertyId && !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(v => v.Status == status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(v =>
                v.VisitorName.ToLower().Contains(s) ||
                v.PassNumber.ToLower().Contains(s) ||
                v.Phone.ToLower().Contains(s) ||
                v.IdNumber.ToLower().Contains(s) ||
                (v.BadgeNumber != null && v.BadgeNumber.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.CheckInTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VisitorLogDto(
                v.Id,
                v.PassNumber,
                v.VisitorName,
                v.IdType,
                v.IdNumber,
                v.Phone,
                v.Purpose,
                v.VisitingDepartment,
                v.RoomOrArea,
                v.CheckInTime,
                v.CheckOutTime,
                v.Status,
                v.BadgeNumber
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<VisitorLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<VisitorLogDto> CheckInVisitorAsync(
        CheckInVisitorRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var countToday = await _context.VisitorLogs.CountAsync(v => v.PropertyId == propertyId && v.CreatedAt >= DateTimeOffset.UtcNow.Date, cancellationToken);
        var passNumber = $"PASS-{DateTime.UtcNow:yyyyMMdd}-{(countToday + 1):D3}";

        var visitor = new VisitorLog
        {
            PropertyId = propertyId,
            PassNumber = passNumber,
            VisitorName = request.VisitorName.Trim(),
            IdType = request.IdType.Trim().ToUpperInvariant(),
            IdNumber = request.IdNumber.Trim(),
            Phone = request.Phone.Trim(),
            Purpose = request.Purpose.Trim(),
            VisitingDepartment = request.VisitingDepartment.Trim().ToUpperInvariant(),
            RoomOrArea = request.RoomOrArea?.Trim(),
            BadgeNumber = request.BadgeNumber?.Trim(),
            CheckInTime = DateTimeOffset.UtcNow,
            Status = "IN_PREMISES",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.VisitorLogs.Add(visitor);
        await _context.SaveChangesAsync(cancellationToken);

        return new VisitorLogDto(
            visitor.Id,
            visitor.PassNumber,
            visitor.VisitorName,
            visitor.IdType,
            visitor.IdNumber,
            visitor.Phone,
            visitor.Purpose,
            visitor.VisitingDepartment,
            visitor.RoomOrArea,
            visitor.CheckInTime,
            visitor.CheckOutTime,
            visitor.Status,
            visitor.BadgeNumber
        );
    }

    public async Task<VisitorLogDto?> CheckOutVisitorAsync(
        long visitorLogId,
        CheckOutVisitorRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var visitor = await _context.VisitorLogs
            .FirstOrDefaultAsync(v => v.Id == visitorLogId && !v.IsDeleted, cancellationToken);

        if (visitor == null) return null;

        visitor.Status = "CHECKED_OUT";
        visitor.CheckOutTime = DateTimeOffset.UtcNow;
        visitor.ModifiedAt = DateTimeOffset.UtcNow;
        visitor.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new VisitorLogDto(
            visitor.Id,
            visitor.PassNumber,
            visitor.VisitorName,
            visitor.IdType,
            visitor.IdNumber,
            visitor.Phone,
            visitor.Purpose,
            visitor.VisitingDepartment,
            visitor.RoomOrArea,
            visitor.CheckInTime,
            visitor.CheckOutTime,
            visitor.Status,
            visitor.BadgeNumber
        );
    }

    public async Task<PagedResult<IncidentReportDto>> GetIncidentsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? severity = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.IncidentReports
            .Include(i => i.ReportedByUser)
            .AsNoTracking()
            .Where(i => i.PropertyId == propertyId && !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(severity))
            query = query.Where(i => i.Severity == severity.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status == status.Trim().ToUpperInvariant());

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.IncidentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new IncidentReportDto(
                i.Id,
                i.IncidentNumber,
                i.IncidentDate,
                i.Location,
                i.Severity,
                i.Category,
                i.Description,
                i.ReportedByUserId,
                i.ReportedByUser != null ? (i.ReportedByUser.FirstName + " " + i.ReportedByUser.LastName).Trim() : null,
                i.Status,
                i.ActionTaken,
                i.ResolvedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<IncidentReportDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IncidentReportDto> ReportIncidentAsync(
        CreateIncidentReportRequest request,
        long propertyId,
        long? userId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var countToday = await _context.IncidentReports.CountAsync(i => i.PropertyId == propertyId && i.CreatedAt >= DateTimeOffset.UtcNow.Date, cancellationToken);
        var incidentNumber = $"INC-{DateTime.UtcNow:yyyyMMdd}-{(countToday + 1):D3}";

        var incident = new IncidentReport
        {
            PropertyId = propertyId,
            IncidentNumber = incidentNumber,
            IncidentDate = request.IncidentDate.ToUniversalTime(),
            Location = request.Location.Trim(),
            Severity = request.Severity.Trim().ToUpperInvariant(),
            Category = request.Category.Trim().ToUpperInvariant(),
            Description = request.Description.Trim(),
            ReportedByUserId = userId,
            Status = "REPORTED",
            ActionTaken = request.ActionTaken?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.IncidentReports.Add(incident);
        await _context.SaveChangesAsync(cancellationToken);

        var user = userId.HasValue ? await _context.Users.FindAsync(new object[] { userId.Value }, cancellationToken) : null;

        return new IncidentReportDto(
            incident.Id,
            incident.IncidentNumber,
            incident.IncidentDate,
            incident.Location,
            incident.Severity,
            incident.Category,
            incident.Description,
            incident.ReportedByUserId,
            user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
            incident.Status,
            incident.ActionTaken,
            incident.ResolvedAt
        );
    }

    public async Task<IncidentReportDto?> UpdateIncidentStatusAsync(
        long incidentId,
        UpdateIncidentStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var incident = await _context.IncidentReports
            .Include(i => i.ReportedByUser)
            .FirstOrDefaultAsync(i => i.Id == incidentId && !i.IsDeleted, cancellationToken);

        if (incident == null) return null;

        var newStatus = request.Status.Trim().ToUpperInvariant();
        incident.Status = newStatus;
        if (!string.IsNullOrWhiteSpace(request.ActionTaken))
            incident.ActionTaken = request.ActionTaken.Trim();

        if (newStatus is "RESOLVED" or "CLOSED")
        {
            incident.ResolvedAt = DateTimeOffset.UtcNow;
        }

        incident.ModifiedAt = DateTimeOffset.UtcNow;
        incident.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new IncidentReportDto(
            incident.Id,
            incident.IncidentNumber,
            incident.IncidentDate,
            incident.Location,
            incident.Severity,
            incident.Category,
            incident.Description,
            incident.ReportedByUserId,
            incident.ReportedByUser != null ? $"{incident.ReportedByUser.FirstName} {incident.ReportedByUser.LastName}".Trim() : null,
            incident.Status,
            incident.ActionTaken,
            incident.ResolvedAt
        );
    }
}

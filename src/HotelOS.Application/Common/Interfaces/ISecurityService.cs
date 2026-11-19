using HotelOS.Contracts.Common;
using HotelOS.Contracts.Security;

namespace HotelOS.Application.Common.Interfaces;

public interface ISecurityService
{
    // Visitor Access
    Task<PagedResult<VisitorLogDto>> GetVisitorsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<VisitorLogDto> CheckInVisitorAsync(
        CheckInVisitorRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<VisitorLogDto?> CheckOutVisitorAsync(
        long visitorLogId,
        CheckOutVisitorRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    // Incidents
    Task<PagedResult<IncidentReportDto>> GetIncidentsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? severity = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<IncidentReportDto> ReportIncidentAsync(
        CreateIncidentReportRequest request,
        long propertyId,
        long? userId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<IncidentReportDto?> UpdateIncidentStatusAsync(
        long incidentId,
        UpdateIncidentStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

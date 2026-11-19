using HotelOS.Contracts.Reporting;

namespace HotelOS.Application.Common.Interfaces;

public interface IReportingService
{
    Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync(
        long propertyId = 1,
        CancellationToken cancellationToken = default);

    Task<NightAuditReportDto> GetNightAuditReportAsync(
        long propertyId = 1,
        DateOnly? auditDate = null,
        CancellationToken cancellationToken = default);

    Task<RevenueTrialBalanceReportDto> GetRevenueTrialBalanceReportAsync(
        long propertyId = 1,
        CancellationToken cancellationToken = default);

    Task<OccupancyForecastReportDto> GetOccupancyForecastReportAsync(
        long propertyId = 1,
        int days = 14,
        CancellationToken cancellationToken = default);

    Task<DepartmentProcurementReportDto> GetDepartmentProcurementReportAsync(
        long propertyId = 1,
        CancellationToken cancellationToken = default);

    Task<StaffPayrollSummaryReportDto> GetStaffPayrollReportAsync(
        long propertyId = 1,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportReportCsvAsync(
        string reportCode,
        long propertyId = 1,
        CancellationToken cancellationToken = default);
}

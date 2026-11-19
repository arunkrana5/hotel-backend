using HotelOS.Contracts.Common;
using HotelOS.Contracts.Maintenance;

namespace HotelOS.Application.Common.Interfaces;

public interface IMaintenanceService
{
    // Assets
    Task<PagedResult<AssetDto>> GetAssetsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? category = null,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<AssetDto> CreateAssetAsync(
        CreateAssetRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    // Work Orders
    Task<PagedResult<WorkOrderDto>> GetWorkOrdersPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? priority = null,
        string? department = null,
        CancellationToken cancellationToken = default);

    Task<WorkOrderDto> CreateWorkOrderAsync(
        CreateWorkOrderRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<WorkOrderDto?> UpdateWorkOrderStatusAsync(
        long workOrderId,
        UpdateWorkOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

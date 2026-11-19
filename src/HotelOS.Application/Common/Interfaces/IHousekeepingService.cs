using HotelOS.Contracts.Common;
using HotelOS.Contracts.Housekeeping;

namespace HotelOS.Application.Common.Interfaces;

public interface IHousekeepingService
{
    Task<PagedResult<HousekeepingTaskDto>> GetTasksPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? taskType = null,
        string? priority = null,
        long? roomId = null,
        CancellationToken cancellationToken = default);

    Task<HousekeepingTaskDto> CreateTaskAsync(
        CreateHousekeepingTaskRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<HousekeepingTaskDto?> UpdateTaskStatusAsync(
        long taskId,
        UpdateHousekeepingTaskStatusRequest request,
        long? currentUserId,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<HousekeepingTaskDto?> AssignTaskAsync(
        long taskId,
        AssignHousekeepingTaskRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<List<LinenItemDto>> GetLinenItemsAsync(
        long propertyId = 1,
        string? category = null,
        CancellationToken cancellationToken = default);

    Task<LinenItemDto?> UpdateLinenStockAsync(
        long linenId,
        UpdateLinenStockRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<PagedResult<LaundryOrderDto>> GetLaundryOrdersPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? orderType = null,
        CancellationToken cancellationToken = default);

    Task<LaundryOrderDto> CreateLaundryOrderAsync(
        CreateLaundryOrderRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<LaundryOrderDto?> UpdateLaundryStatusAsync(
        long orderId,
        UpdateLaundryStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

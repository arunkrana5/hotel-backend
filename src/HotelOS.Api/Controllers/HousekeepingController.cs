using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Housekeeping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/housekeeping")]
[Authorize]
public class HousekeepingController : ControllerBase
{
    private readonly IHousekeepingService _housekeepingService;
    private readonly ICurrentUserService _currentUser;

    public HousekeepingController(
        IHousekeepingService housekeepingService,
        ICurrentUserService currentUser)
    {
        _housekeepingService = housekeepingService;
        _currentUser = currentUser;
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? taskType = null,
        [FromQuery] string? priority = null,
        [FromQuery] long? roomId = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _housekeepingService.GetTasksPagedAsync(
            propertyId, page, pageSize, status, taskType, priority, roomId, cancellationToken);
        return Ok(ApiResponse<PagedResult<HousekeepingTaskDto>>.Ok(result));
    }

    [HttpPost("tasks")]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateHousekeepingTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _housekeepingService.CreateTaskAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<HousekeepingTaskDto>.Ok(result, "Housekeeping task created successfully."));
    }

    [HttpPut("tasks/{id}/status")]
    public async Task<IActionResult> UpdateTaskStatus(
        [FromRoute] long id,
        [FromBody] UpdateHousekeepingTaskStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        long? userId = _currentUser.UserId;
        var result = await _housekeepingService.UpdateTaskStatusAsync(id, request, userId, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Task #{id} not found.", 404));

        return Ok(ApiResponse<HousekeepingTaskDto>.Ok(result, "Task status updated successfully."));
    }

    [HttpPut("tasks/{id}/assign")]
    public async Task<IActionResult> AssignTask(
        [FromRoute] long id,
        [FromBody] AssignHousekeepingTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _housekeepingService.AssignTaskAsync(id, request, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Task #{id} not found.", 404));

        return Ok(ApiResponse<HousekeepingTaskDto>.Ok(result, "Task assigned successfully."));
    }

    [HttpGet("linen")]
    public async Task<IActionResult> GetLinenItems(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _housekeepingService.GetLinenItemsAsync(propertyId, category, cancellationToken);
        return Ok(ApiResponse<List<LinenItemDto>>.Ok(result));
    }

    [HttpPut("linen/{id}/stock")]
    public async Task<IActionResult> UpdateLinenStock(
        [FromRoute] long id,
        [FromBody] UpdateLinenStockRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _housekeepingService.UpdateLinenStockAsync(id, request, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Linen item #{id} not found.", 404));

        return Ok(ApiResponse<LinenItemDto>.Ok(result, "Linen stock updated successfully."));
    }

    [HttpGet("laundry")]
    public async Task<IActionResult> GetLaundryOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? orderType = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _housekeepingService.GetLaundryOrdersPagedAsync(
            propertyId, page, pageSize, status, orderType, cancellationToken);
        return Ok(ApiResponse<PagedResult<LaundryOrderDto>>.Ok(result));
    }

    [HttpPost("laundry")]
    public async Task<IActionResult> CreateLaundryOrder(
        [FromBody] CreateLaundryOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _housekeepingService.CreateLaundryOrderAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<LaundryOrderDto>.Ok(result, "Laundry order placed successfully."));
    }

    [HttpPut("laundry/{id}/status")]
    public async Task<IActionResult> UpdateLaundryStatus(
        [FromRoute] long id,
        [FromBody] UpdateLaundryStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _housekeepingService.UpdateLaundryStatusAsync(id, request, user, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Laundry order #{id} not found.", 404));

        return Ok(ApiResponse<LaundryOrderDto>.Ok(result, "Laundry order status updated."));
    }
}

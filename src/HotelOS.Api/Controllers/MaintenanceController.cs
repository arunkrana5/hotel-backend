using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Maintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/maintenance")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ICurrentUserService _currentUser;

    public MaintenanceController(
        IMaintenanceService maintenanceService,
        ICurrentUserService currentUser)
    {
        _maintenanceService = maintenanceService;
        _currentUser = currentUser;
    }

    [HttpGet("assets")]
    public async Task<IActionResult> GetAssets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _maintenanceService.GetAssetsPagedAsync(propertyId, page, pageSize, category, status, search, cancellationToken);
        return Ok(ApiResponse<PagedResult<AssetDto>>.Ok(result));
    }

    [HttpPost("assets")]
    public async Task<IActionResult> CreateAsset(
        [FromBody] CreateAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _maintenanceService.CreateAssetAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<AssetDto>.Ok(result, "Asset registered successfully."));
    }

    [HttpGet("work-orders")]
    public async Task<IActionResult> GetWorkOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? department = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _maintenanceService.GetWorkOrdersPagedAsync(propertyId, page, pageSize, status, priority, department, cancellationToken);
        return Ok(ApiResponse<PagedResult<WorkOrderDto>>.Ok(result));
    }

    [HttpPost("work-orders")]
    public async Task<IActionResult> CreateWorkOrder(
        [FromBody] CreateWorkOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _maintenanceService.CreateWorkOrderAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<WorkOrderDto>.Ok(result, "Work order logged successfully."));
    }

    [HttpPut("work-orders/{id}/status")]
    public async Task<IActionResult> UpdateWorkOrderStatus(
        [FromRoute] long id,
        [FromBody] UpdateWorkOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _maintenanceService.UpdateWorkOrderStatusAsync(id, request, user, cancellationToken);
        if (result == null) return NotFound(ApiResponse<WorkOrderDto>.Fail("Work order not found."));
        return Ok(ApiResponse<WorkOrderDto>.Ok(result, "Work order status updated."));
    }
}

using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Purchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/purchase")]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly ICurrentUserService _currentUser;

    public PurchaseController(
        IPurchaseService purchaseService,
        ICurrentUserService currentUser)
    {
        _purchaseService = purchaseService;
        _currentUser = currentUser;
    }

    [HttpGet("vendors")]
    public async Task<IActionResult> GetVendors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _purchaseService.GetVendorsPagedAsync(propertyId, page, pageSize, search, status, cancellationToken);
        return Ok(ApiResponse<PagedResult<VendorDto>>.Ok(result));
    }

    [HttpPost("vendors")]
    public async Task<IActionResult> CreateVendor(
        [FromBody] CreateVendorRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _purchaseService.CreateVendorAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<VendorDto>.Ok(result, "Vendor registered successfully."));
    }

    [HttpPut("vendors/{id:long}")]
    public async Task<IActionResult> UpdateVendor(
        [FromRoute] long id,
        [FromBody] UpdateVendorRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _purchaseService.UpdateVendorAsync(id, request, user, cancellationToken);
        return Ok(ApiResponse<VendorDto>.Ok(result, "Vendor updated successfully."));
    }

    [HttpGet("requisitions")]
    public async Task<IActionResult> GetRequisitions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? department = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _purchaseService.GetRequisitionsPagedAsync(propertyId, page, pageSize, department, status, cancellationToken);
        return Ok(ApiResponse<PagedResult<PurchaseRequisitionDto>>.Ok(result));
    }

    [HttpPost("requisitions")]
    public async Task<IActionResult> CreateRequisition(
        [FromBody] CreatePurchaseRequisitionRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        long userId = _currentUser.UserId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _purchaseService.CreateRequisitionAsync(request, propertyId, userId, user, cancellationToken);
        return Ok(ApiResponse<PurchaseRequisitionDto>.Ok(result, "Requisition created successfully."));
    }

    [HttpPut("requisitions/{id}/status")]
    public async Task<IActionResult> UpdateRequisitionStatus(
        [FromRoute] long id,
        [FromBody] UpdateRequisitionStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _purchaseService.UpdateRequisitionStatusAsync(id, request, user, cancellationToken);
        if (result == null) return NotFound(ApiResponse<PurchaseRequisitionDto>.Fail("Requisition not found."));
        return Ok(ApiResponse<PurchaseRequisitionDto>.Ok(result, "Requisition status updated."));
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] long? vendorId = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _purchaseService.GetOrdersPagedAsync(propertyId, page, pageSize, status, vendorId, cancellationToken);
        return Ok(ApiResponse<PagedResult<PurchaseOrderDto>>.Ok(result));
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _purchaseService.CreateOrderAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result, "Purchase order generated successfully."));
    }

    [HttpPut("orders/{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromRoute] long id,
        [FromBody] UpdatePurchaseOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _purchaseService.UpdateOrderStatusAsync(id, request, user, cancellationToken);
        if (result == null) return NotFound(ApiResponse<PurchaseOrderDto>.Fail("Purchase order not found."));
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result, "Purchase order status updated."));
    }
}

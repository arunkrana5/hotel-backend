using HotelOS.Contracts.Common;
using HotelOS.Contracts.Purchase;

namespace HotelOS.Application.Common.Interfaces;

public interface IPurchaseService
{
    // Vendors
    Task<PagedResult<VendorDto>> GetVendorsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<VendorDto> CreateVendorAsync(
        CreateVendorRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    // Requisitions
    Task<PagedResult<PurchaseRequisitionDto>> GetRequisitionsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? department = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<PurchaseRequisitionDto> CreateRequisitionAsync(
        CreatePurchaseRequisitionRequest request,
        long propertyId,
        long userId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<PurchaseRequisitionDto?> UpdateRequisitionStatusAsync(
        long requisitionId,
        UpdateRequisitionStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    // Purchase Orders
    Task<PagedResult<PurchaseOrderDto>> GetOrdersPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        long? vendorId = null,
        CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto> CreateOrderAsync(
        CreatePurchaseOrderRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto?> UpdateOrderStatusAsync(
        long orderId,
        UpdatePurchaseOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

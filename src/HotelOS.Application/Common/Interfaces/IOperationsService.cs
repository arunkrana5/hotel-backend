using HotelOS.Contracts.Common;
using HotelOS.Contracts.Operations;

namespace HotelOS.Application.Common.Interfaces;

public interface IOperationsService
{
    Task<PagedResult<GuestServiceRequestDto>> GetGuestRequestsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? department = null,
        string? priority = null,
        CancellationToken cancellationToken = default);

    Task<GuestServiceRequestDto> CreateGuestRequestAsync(
        CreateGuestServiceRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<GuestServiceRequestDto?> UpdateGuestRequestStatusAsync(
        long requestId,
        UpdateGuestServiceRequestStatus request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

using HotelOS.Contracts.Banquet;
using HotelOS.Contracts.Common;

namespace HotelOS.Application.Common.Interfaces;

public interface IBanquetService
{
    Task<List<BanquetVenueDto>> GetVenuesAsync(
        long propertyId = 1,
        CancellationToken cancellationToken = default);

    Task<PagedResult<BanquetBookingDto>> GetBookingsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default);

    Task<BanquetBookingDto> CreateBookingAsync(
        CreateBanquetBookingRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<BanquetBookingDto?> UpdateBookingStatusAsync(
        long bookingId,
        UpdateBanquetBookingStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

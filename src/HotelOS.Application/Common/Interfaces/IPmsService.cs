using HotelOS.Contracts.Common;
using HotelOS.Contracts.Pms;

namespace HotelOS.Application.Common.Interfaces;

public interface IPmsService
{
    // Room & Front Desk Grid
    Task<RoomGridSummaryDto> GetRoomGridAsync(
        long propertyId = 1,
        int? floor = null,
        string? status = null,
        string? roomType = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<RoomDto>> GetRoomsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? status = null,
        string? roomType = null,
        CancellationToken cancellationToken = default);

    Task<RoomDto?> UpdateRoomStatusAsync(
        long roomId,
        string statusCode,
        string? notes,
        string updatedBy,
        CancellationToken cancellationToken = default);

    // Guest Directory & CRM
    Task<PagedResult<GuestDto>> GetGuestsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? vipStatus = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<GuestDto> CreateGuestAsync(
        CreateGuestRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<GuestDto?> GetGuestByIdAsync(
        long guestId,
        CancellationToken cancellationToken = default);

    Task<GuestDto> UpdateGuestAsync(
        long id,
        UpdateGuestRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    Task<bool> SetGuestStatusAsync(
        long id,
        bool isActive,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> DeleteGuestAsync(
        long id,
        string deletedBy,
        CancellationToken cancellationToken = default);

    Task<GuestCrmProfileDto?> GetGuestCrmProfileAsync(
        long guestId,
        CancellationToken cancellationToken = default);

    Task<GuestCrmStatsDto> GetGuestCrmStatsAsync(
        long propertyId = 1,
        CancellationToken cancellationToken = default);

    Task<DuplicateCheckResult> CheckDuplicateGuestAsync(
        string email,
        string phone,
        long? excludeId = null,
        long propertyId = 1,
        CancellationToken cancellationToken = default);

    // Reservations
    Task<PagedResult<ReservationDto>> GetReservationsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? status = null,
        string? checkInDate = null,
        string? checkOutDate = null,
        CancellationToken cancellationToken = default);

    Task<ReservationDto> CreateReservationAsync(
        CreateReservationRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<ReservationDto?> GetReservationByIdAsync(
        long reservationId,
        CancellationToken cancellationToken = default);

    // Front Desk Lifecycle (Check-in / Check-out)
    Task<ReservationDto> CheckInGuestAsync(
        CheckInRequest request,
        string checkedInBy,
        CancellationToken cancellationToken = default);

    Task<ReservationDto> CheckOutGuestAsync(
        CheckOutRequest request,
        string checkedOutBy,
        CancellationToken cancellationToken = default);

    // Folios & Billing
    Task<FolioDto?> GetFolioByReservationIdAsync(
        long reservationId,
        CancellationToken cancellationToken = default);

    Task<FolioDto?> GetFolioByIdAsync(
        long folioId,
        CancellationToken cancellationToken = default);

    Task<FolioDto> PostChargeAsync(
        PostChargeRequest request,
        string postedBy,
        CancellationToken cancellationToken = default);

    Task<FolioDto> PostPaymentAsync(
        PostPaymentRequest request,
        string postedBy,
        CancellationToken cancellationToken = default);
}

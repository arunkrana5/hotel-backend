using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Banquet;
using HotelOS.Contracts.Common;
using HotelOS.Domain.Banquet.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class BanquetService : IBanquetService
{
    private readonly HotelOSDbContext _context;

    public BanquetService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<List<BanquetVenueDto>> GetVenuesAsync(
        long propertyId = 1,
        CancellationToken cancellationToken = default)
    {
        return await _context.BanquetVenues
            .AsNoTracking()
            .Where(v => v.PropertyId == propertyId && !v.IsDeleted)
            .OrderBy(v => v.VenueName)
            .Select(v => new BanquetVenueDto(
                v.Id,
                v.VenueCode,
                v.VenueName,
                v.Location,
                v.MaxCapacity,
                v.HourlyRate,
                v.DayRate,
                v.Status,
                v.AmenitiesJson
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<BanquetBookingDto>> GetBookingsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.BanquetBookings
            .Include(b => b.Venue)
            .Include(b => b.Guest)
            .AsNoTracking()
            .Where(b => b.PropertyId == propertyId && !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.Status == status.Trim().ToUpperInvariant());

        if (fromDate.HasValue)
            query = query.Where(b => b.StartDateTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(b => b.EndDateTime <= toDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var bookings = await query
            .OrderByDescending(b => b.StartDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BanquetBookingDto(
                b.Id,
                b.BookingNumber,
                b.VenueId,
                b.Venue != null ? b.Venue.VenueName : string.Empty,
                b.EventName,
                b.EventType,
                b.ClientName,
                b.ClientPhone,
                b.ClientEmail,
                b.GuestId,
                b.Guest != null ? $"{b.Guest.FirstName} {b.Guest.LastName}" : null,
                b.StartDateTime,
                b.EndDateTime,
                b.ExpectedGuests,
                b.Status,
                b.TotalAmount,
                b.AdvancePaid,
                b.BalanceAmount,
                b.CateringPackage,
                b.SetupStyle,
                b.SpecialRequests,
                b.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<BanquetBookingDto>
        {
            Items = bookings,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<BanquetBookingDto> CreateBookingAsync(
        CreateBanquetBookingRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var venue = await _context.BanquetVenues.FindAsync([request.VenueId], cancellationToken)
            ?? throw new KeyNotFoundException($"Venue with ID {request.VenueId} not found.");

        var countToday = await _context.BanquetBookings
            .CountAsync(b => b.PropertyId == propertyId && b.CreatedAt.Date == DateTimeOffset.UtcNow.Date, cancellationToken);

        var bookingNumber = $"BNQ-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";

        var balance = request.TotalAmount - request.AdvancePaid;

        var booking = new BanquetBooking
        {
            PropertyId = propertyId,
            BookingNumber = bookingNumber,
            VenueId = request.VenueId,
            EventName = request.EventName,
            EventType = request.EventType.Trim().ToUpperInvariant(),
            ClientName = request.ClientName,
            ClientPhone = request.ClientPhone,
            ClientEmail = request.ClientEmail,
            GuestId = request.GuestId,
            StartDateTime = request.StartDateTime.ToUniversalTime(),
            EndDateTime = request.EndDateTime.ToUniversalTime(),
            ExpectedGuests = request.ExpectedGuests,
            Status = request.AdvancePaid > 0 ? "CONFIRMED" : "INQUIRY",
            TotalAmount = request.TotalAmount,
            AdvancePaid = request.AdvancePaid,
            BalanceAmount = balance,
            CateringPackage = request.CateringPackage,
            SetupStyle = request.SetupStyle,
            SpecialRequests = request.SpecialRequests,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.BanquetBookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);

        string? guestName = null;
        if (booking.GuestId.HasValue)
        {
            var guest = await _context.Guests.FindAsync([booking.GuestId.Value], cancellationToken);
            guestName = guest != null ? $"{guest.FirstName} {guest.LastName}" : null;
        }

        return new BanquetBookingDto(
            booking.Id,
            booking.BookingNumber,
            booking.VenueId,
            venue.VenueName,
            booking.EventName,
            booking.EventType,
            booking.ClientName,
            booking.ClientPhone,
            booking.ClientEmail,
            booking.GuestId,
            guestName,
            booking.StartDateTime,
            booking.EndDateTime,
            booking.ExpectedGuests,
            booking.Status,
            booking.TotalAmount,
            booking.AdvancePaid,
            booking.BalanceAmount,
            booking.CateringPackage,
            booking.SetupStyle,
            booking.SpecialRequests,
            booking.CreatedAt
        );
    }

    public async Task<BanquetBookingDto?> UpdateBookingStatusAsync(
        long bookingId,
        UpdateBanquetBookingStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var booking = await _context.BanquetBookings
            .Include(b => b.Venue)
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

        if (booking == null) return null;

        booking.Status = request.Status.Trim().ToUpperInvariant();
        booking.ModifiedAt = DateTimeOffset.UtcNow;
        booking.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new BanquetBookingDto(
            booking.Id,
            booking.BookingNumber,
            booking.VenueId,
            booking.Venue != null ? booking.Venue.VenueName : string.Empty,
            booking.EventName,
            booking.EventType,
            booking.ClientName,
            booking.ClientPhone,
            booking.ClientEmail,
            booking.GuestId,
            booking.Guest != null ? $"{booking.Guest.FirstName} {booking.Guest.LastName}" : null,
            booking.StartDateTime,
            booking.EndDateTime,
            booking.ExpectedGuests,
            booking.Status,
            booking.TotalAmount,
            booking.AdvancePaid,
            booking.BalanceAmount,
            booking.CateringPackage,
            booking.SetupStyle,
            booking.SpecialRequests,
            booking.CreatedAt
        );
    }
}

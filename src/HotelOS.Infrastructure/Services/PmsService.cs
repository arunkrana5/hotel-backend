using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Pms;
using HotelOS.Domain.Pms.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class PmsService : IPmsService
{
    private readonly HotelOSDbContext _context;

    public PmsService(HotelOSDbContext context)
    {
        _context = context;
    }

    #region Room & Grid Operations

    public async Task<RoomGridSummaryDto> GetRoomGridAsync(
        long propertyId = 1,
        int? floor = null,
        string? status = null,
        string? roomType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Rooms
            .AsNoTracking()
            .Where(r => r.PropertyId == propertyId && !r.IsDeleted);

        if (floor.HasValue)
            query = query.Where(r => r.FloorNumber == floor.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.RoomStatusCode == status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(roomType))
            query = query.Where(r => r.RoomTypeCode == roomType.Trim().ToUpperInvariant());

        var allRooms = await query
            .OrderBy(r => r.FloorNumber)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync(cancellationToken);

        int total = allRooms.Count;
        int occupied = allRooms.Count(r => r.RoomStatusCode == "OCC");
        int clean = allRooms.Count(r => r.RoomStatusCode == "CLN");
        int dirty = allRooms.Count(r => r.RoomStatusCode == "DRT");
        int inspected = allRooms.Count(r => r.RoomStatusCode == "INS");
        int maintenance = allRooms.Count(r => r.RoomStatusCode == "MNT");
        int outOfOrder = allRooms.Count(r => r.RoomStatusCode == "OOD");

        var floors = allRooms
            .GroupBy(r => r.FloorNumber)
            .OrderBy(g => g.Key)
            .Select(g => new FloorRoomsDto(
                g.Key,
                g.Select(r => new RoomDto(
                    r.Id,
                    r.RoomNumber,
                    r.FloorNumber,
                    r.RoomTypeCode,
                    r.RoomStatusCode,
                    r.MaxOccupancy,
                    r.BaseRatePerNight,
                    r.IsSmoking,
                    r.KeyCardUid,
                    r.Notes
                )).ToList()
            )).ToList();

        return new RoomGridSummaryDto(
            total,
            occupied,
            clean,
            dirty,
            inspected,
            maintenance,
            outOfOrder,
            floors
        );
    }

    public async Task<PagedResult<RoomDto>> GetRoomsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? status = null,
        string? roomType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Rooms
            .AsNoTracking()
            .Where(r => r.PropertyId == propertyId && !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(r => r.RoomNumber.Contains(term) || (r.Notes != null && r.Notes.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.RoomStatusCode == status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(roomType))
            query = query.Where(r => r.RoomTypeCode == roomType.Trim().ToUpperInvariant());

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(r => r.FloorNumber)
            .ThenBy(r => r.RoomNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoomDto(
                r.Id,
                r.RoomNumber,
                r.FloorNumber,
                r.RoomTypeCode,
                r.RoomStatusCode,
                r.MaxOccupancy,
                r.BaseRatePerNight,
                r.IsSmoking,
                r.KeyCardUid,
                r.Notes
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<RoomDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<RoomDto?> UpdateRoomStatusAsync(
        long roomId,
        string statusCode,
        string? notes,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);
        if (room == null) return null;

        room.RoomStatusCode = statusCode.Trim().ToUpperInvariant();
        if (notes != null) room.Notes = notes;
        room.ModifiedBy = updatedBy;
        room.ModifiedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new RoomDto(
            room.Id,
            room.RoomNumber,
            room.FloorNumber,
            room.RoomTypeCode,
            room.RoomStatusCode,
            room.MaxOccupancy,
            room.BaseRatePerNight,
            room.IsSmoking,
            room.KeyCardUid,
            room.Notes
        );
    }

    #endregion

    #region Guest Operations

    public async Task<PagedResult<GuestDto>> GetGuestsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? vipStatus = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Guests
            .AsNoTracking()
            .Where(g => g.PropertyId == propertyId && !g.IsDeleted);

        if (isActive.HasValue)
        {
            query = query.Where(g => g.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim().ToLower();
            query = query.Where(g =>
                g.FirstName.ToLower().Contains(term) ||
                g.LastName.ToLower().Contains(term) ||
                g.Email.ToLower().Contains(term) ||
                g.Phone.Contains(term) ||
                (g.IdDocumentNumber != null && g.IdDocumentNumber.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(vipStatus) && vipStatus != "ALL")
        {
            if (vipStatus == "VIP")
                query = query.Where(g => g.VipStatus == "VIP" || g.VipStatus == "Platinum" || g.VipStatus == "Gold");
            else
                query = query.Where(g => g.VipStatus == vipStatus.Trim());
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GuestDto(
                g.Id,
                g.FirstName,
                g.LastName,
                g.FirstName + " " + g.LastName,
                g.Email,
                g.Phone,
                g.IdDocumentType,
                g.IdDocumentNumber,
                g.Nationality,
                g.VipStatus,
                g.TotalStays,
                g.LifetimeSpend,
                g.SpecialPreferences,
                g.City,
                g.Country,
                g.IsActive,
                g.Address
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<GuestDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<GuestDto> CreateGuestAsync(
        CreateGuestRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var guest = new Guest
        {
            PropertyId = propertyId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone.Trim(),
            IdDocumentType = request.IdDocumentType,
            IdDocumentNumber = request.IdDocumentNumber?.Trim(),
            Nationality = request.Nationality?.Trim(),
            VipStatus = string.IsNullOrWhiteSpace(request.VipStatus) ? "Standard" : request.VipStatus.Trim(),
            SpecialPreferences = request.SpecialPreferences?.Trim(),
            Address = request.Address?.Trim(),
            City = request.City?.Trim(),
            Country = request.Country?.Trim(),
            IsActive = true,
            CreatedBy = createdBy
        };

        _context.Guests.Add(guest);
        await _context.SaveChangesAsync(cancellationToken);

        return new GuestDto(
            guest.Id,
            guest.FirstName,
            guest.LastName,
            guest.FirstName + " " + guest.LastName,
            guest.Email,
            guest.Phone,
            guest.IdDocumentType,
            guest.IdDocumentNumber,
            guest.Nationality,
            guest.VipStatus,
            guest.TotalStays,
            guest.LifetimeSpend,
            guest.SpecialPreferences,
            guest.City,
            guest.Country,
            guest.IsActive,
            guest.Address
        );
    }

    public async Task<GuestDto?> GetGuestByIdAsync(long guestId, CancellationToken cancellationToken = default)
    {
        var g = await _context.Guests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == guestId && !x.IsDeleted, cancellationToken);

        if (g == null) return null;

        return new GuestDto(
            g.Id,
            g.FirstName,
            g.LastName,
            g.FirstName + " " + g.LastName,
            g.Email,
            g.Phone,
            g.IdDocumentType,
            g.IdDocumentNumber,
            g.Nationality,
            g.VipStatus,
            g.TotalStays,
            g.LifetimeSpend,
            g.SpecialPreferences,
            g.City,
            g.Country,
            g.IsActive,
            g.Address
        );
    }

    public async Task<GuestDto> UpdateGuestAsync(
        long id,
        UpdateGuestRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var guest = await _context.Guests.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted, cancellationToken);
        if (guest == null)
            throw new KeyNotFoundException($"Guest #{id} not found.");

        guest.FirstName = request.FirstName.Trim();
        guest.LastName = request.LastName.Trim();
        guest.Email = request.Email.Trim().ToLowerInvariant();
        guest.Phone = request.Phone.Trim();
        guest.IdDocumentType = request.IdDocumentType;
        guest.IdDocumentNumber = request.IdDocumentNumber?.Trim();
        guest.Nationality = request.Nationality?.Trim();
        guest.VipStatus = string.IsNullOrWhiteSpace(request.VipStatus) ? "Standard" : request.VipStatus.Trim();
        guest.SpecialPreferences = request.SpecialPreferences?.Trim();
        guest.Address = request.Address?.Trim();
        guest.City = request.City?.Trim();
        guest.Country = request.Country?.Trim();
        guest.IsActive = request.IsActive;
        guest.ModifiedAt = DateTimeOffset.UtcNow;
        guest.ModifiedBy = modifiedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new GuestDto(
            guest.Id,
            guest.FirstName,
            guest.LastName,
            guest.FirstName + " " + guest.LastName,
            guest.Email,
            guest.Phone,
            guest.IdDocumentType,
            guest.IdDocumentNumber,
            guest.Nationality,
            guest.VipStatus,
            guest.TotalStays,
            guest.LifetimeSpend,
            guest.SpecialPreferences,
            guest.City,
            guest.Country,
            guest.IsActive,
            guest.Address
        );
    }

    public async Task<bool> SetGuestStatusAsync(
        long id,
        bool isActive,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var guest = await _context.Guests.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted, cancellationToken);
        if (guest == null) return false;

        guest.IsActive = isActive;
        guest.ModifiedAt = DateTimeOffset.UtcNow;
        guest.ModifiedBy = modifiedBy;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(bool Success, string Message)> DeleteGuestAsync(
        long id,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        var guest = await _context.Guests.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted, cancellationToken);
        if (guest == null)
            return (false, $"Guest #{id} not found.");

        // Check if guest has active reservations or non-zero folio balances
        bool hasActiveBookings = await _context.Reservations.AnyAsync(r =>
            r.GuestId == id && !r.IsDeleted &&
            r.ReservationStatus != "CANCELLED" && r.ReservationStatus != "CHECKED_OUT",
            cancellationToken);

        if (hasActiveBookings)
        {
            return (false, "Cannot delete guest profile with active or upcoming reservations. Consider deactivating the profile instead.");
        }

        bool hasOutstandingFolios = await _context.Folios.AnyAsync(f =>
            f.GuestId == id && !f.IsDeleted && f.BalanceAmount != 0,
            cancellationToken);

        if (hasOutstandingFolios)
        {
            return (false, "Cannot delete guest profile with outstanding folio balances. Please settle open balances prior to deletion.");
        }

        // Soft delete
        guest.IsDeleted = true;
        guest.DeletedAt = DateTimeOffset.UtcNow;
        guest.DeletedBy = deletedBy;
        await _context.SaveChangesAsync(cancellationToken);

        return (true, "Guest profile archived successfully.");
    }

    public async Task<GuestCrmProfileDto?> GetGuestCrmProfileAsync(
        long guestId,
        CancellationToken cancellationToken = default)
    {
        var guest = await _context.Guests
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == guestId && !g.IsDeleted, cancellationToken);

        if (guest == null) return null;

        var reservations = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.GuestId == guestId && !r.IsDeleted)
            .OrderByDescending(r => r.CheckInDate)
            .ToListAsync(cancellationToken);

        var folios = await _context.Folios
            .Include(f => f.Items)
            .Include(f => f.Reservation)
            .AsNoTracking()
            .Where(f => f.GuestId == guestId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int upcoming = reservations.Count(r => (r.ReservationStatus == "CONF" || r.ReservationStatus == "PROV") && r.CheckInDate >= today);
        int completed = reservations.Count(r => r.ReservationStatus == "CHKO" || r.ActualCheckOutAt != null || r.CheckOutDate < today);

        decimal totalRevenue = folios.Sum(f => f.TotalCharges);
        if (totalRevenue == 0 && reservations.Any())
        {
            totalRevenue = reservations.Sum(r => r.PaidAmount > 0 ? r.PaidAmount : r.TotalEstimatedAmount);
        }

        decimal outstanding = folios.Sum(f => f.BalanceAmount);

        var lastCompletedStay = reservations.FirstOrDefault(r => r.CheckOutDate <= today || r.ActualCheckOutAt != null);
        string? lastStayStr = lastCompletedStay != null ? lastCompletedStay.CheckOutDate.ToString("yyyy-MM-dd") : null;

        double avgDuration = reservations.Any()
            ? reservations.Average(r => Math.Max(1, (r.CheckOutDate.DayNumber - r.CheckInDate.DayNumber)))
            : 0;

        var resDtos = reservations.Select(r => new ReservationDto(
            r.Id,
            r.ConfirmationNumber,
            guest.Id,
            guest.FirstName + " " + guest.LastName,
            guest.Email,
            guest.Phone,
            guest.VipStatus,
            r.RoomTypeCode,
            r.AssignedRoomId,
            r.AssignedRoom != null ? r.AssignedRoom.RoomNumber : null,
            r.CheckInDate.ToString("yyyy-MM-dd"),
            r.CheckOutDate.ToString("yyyy-MM-dd"),
            r.AdultsCount,
            r.ChildrenCount,
            r.ReservationStatus,
            r.BookingSource,
            r.RatePerNight,
            r.TotalEstimatedAmount,
            r.PaidAmount,
            r.TotalEstimatedAmount - r.PaidAmount,
            r.SpecialRequests,
            r.ActualCheckInAt,
            r.ActualCheckOutAt
        )).ToList();

        var stayHistoryDtos = reservations.Select(r => new GuestStayHistoryDto(
            r.Id,
            r.ConfirmationNumber,
            r.RoomTypeCode,
            r.AssignedRoom != null ? r.AssignedRoom.RoomNumber : "Unassigned",
            r.CheckInDate.ToString("yyyy-MM-dd"),
            r.CheckOutDate.ToString("yyyy-MM-dd"),
            Math.Max(1, r.CheckOutDate.DayNumber - r.CheckInDate.DayNumber),
            r.TotalEstimatedAmount,
            r.ReservationStatus
        )).ToList();

        var folioDtos = folios.Select(f => new GuestFolioSummaryDto(
            f.Id,
            f.FolioNumber,
            f.Reservation != null ? f.Reservation.ConfirmationNumber : "Direct Billing",
            f.Status,
            f.TotalCharges,
            f.TotalPayments,
            f.BalanceAmount,
            f.CreatedAt,
            f.Items.Select(i => new GuestFolioItemDto(
                i.Id,
                i.ItemType,
                i.DepartmentCode,
                i.Description,
                i.Amount,
                i.PaymentModeCode,
                i.TransactionReference,
                i.PostedAt
            )).ToList()
        )).ToList();

        var auditLog = new List<GuestAuditEventDto>
        {
            new("PROFILE_CREATED", $"Guest profile created in property system by {guest.CreatedBy ?? "SYSTEM"}", guest.CreatedAt, guest.CreatedBy ?? "SYSTEM")
        };
        if (guest.ModifiedAt.HasValue)
        {
            auditLog.Add(new("PROFILE_UPDATED", $"Profile data and preferences updated by {guest.ModifiedBy ?? "STAFF"}", guest.ModifiedAt.Value, guest.ModifiedBy ?? "STAFF"));
        }
        foreach (var r in reservations.Take(3))
        {
            auditLog.Add(new("RESERVATION_LINKED", $"Reservation #{r.ConfirmationNumber} ({r.RoomTypeCode}) linked to guest profile", r.CreatedAt, r.CreatedBy ?? "FRONT_DESK"));
        }

        var summary = new GuestCrmSummaryDto(
            Math.Max(guest.TotalStays, reservations.Count),
            upcoming,
            completed,
            totalRevenue,
            outstanding,
            lastStayStr,
            Math.Round(avgDuration, 1)
        );

        return new GuestCrmProfileDto(
            guest.Id,
            guest.FirstName,
            guest.LastName,
            guest.FirstName + " " + guest.LastName,
            guest.Email,
            guest.Phone,
            guest.IdDocumentType,
            guest.IdDocumentNumber,
            guest.Nationality,
            guest.VipStatus,
            guest.SpecialPreferences,
            guest.Address,
            guest.City,
            guest.Country,
            guest.IsActive,
            guest.CreatedAt,
            summary,
            resDtos,
            stayHistoryDtos,
            folioDtos,
            auditLog.OrderByDescending(a => a.Timestamp).ToList()
        );
    }

    public async Task<GuestCrmStatsDto> GetGuestCrmStatsAsync(long propertyId = 1, CancellationToken cancellationToken = default)
    {
        var query = _context.Guests.AsNoTracking().Where(g => g.PropertyId == propertyId && !g.IsDeleted);

        int total = await query.CountAsync(cancellationToken);
        int active = await query.CountAsync(g => g.IsActive, cancellationToken);
        int vip = await query.CountAsync(g => g.VipStatus == "VIP" || g.VipStatus == "Platinum" || g.VipStatus == "Gold", cancellationToken);
        int inactive = total - active;

        decimal totalSpend = await query.SumAsync(g => g.LifetimeSpend, cancellationToken);
        decimal avgSpend = total > 0 ? Math.Round(totalSpend / total, 2) : 0m;

        return new GuestCrmStatsDto(total, active, vip, inactive, totalSpend, avgSpend);
    }

    public async Task<DuplicateCheckResult> CheckDuplicateGuestAsync(
        string email,
        string phone,
        long? excludeId = null,
        long propertyId = 1,
        CancellationToken cancellationToken = default)
    {
        string normEmail = email.Trim().ToLowerInvariant();
        string normPhone = phone.Trim();

        var query = _context.Guests
            .AsNoTracking()
            .Where(g => g.PropertyId == propertyId && !g.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(g => g.Id != excludeId.Value);
        }

        var matched = await query.FirstOrDefaultAsync(g =>
            g.Email.ToLower() == normEmail || g.Phone == normPhone,
            cancellationToken);

        if (matched == null)
        {
            return new DuplicateCheckResult(false, null);
        }

        var dto = new GuestDto(
            matched.Id,
            matched.FirstName,
            matched.LastName,
            matched.FirstName + " " + matched.LastName,
            matched.Email,
            matched.Phone,
            matched.IdDocumentType,
            matched.IdDocumentNumber,
            matched.Nationality,
            matched.VipStatus,
            matched.TotalStays,
            matched.LifetimeSpend,
            matched.SpecialPreferences,
            matched.City,
            matched.Country,
            matched.IsActive,
            matched.Address
        );

        return new DuplicateCheckResult(true, dto);
    }

    #endregion

    #region Reservations

    public async Task<PagedResult<ReservationDto>> GetReservationsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? status = null,
        string? checkInDate = null,
        string? checkOutDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Reservations
            .Include(res => res.Guest)
            .Include(res => res.AssignedRoom)
            .Include(res => res.Folios)
            .AsNoTracking()
            .Where(res => res.PropertyId == propertyId && !res.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(res =>
                res.ConfirmationNumber.Contains(term) ||
                res.Guest.FirstName.Contains(term) ||
                res.Guest.LastName.Contains(term) ||
                res.Guest.Phone.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(res => res.ReservationStatus == status.Trim().ToUpperInvariant());

        if (DateOnly.TryParse(checkInDate, out var inDate))
            query = query.Where(res => res.CheckInDate == inDate);

        if (DateOnly.TryParse(checkOutDate, out var outDate))
            query = query.Where(res => res.CheckOutDate == outDate);

        int totalCount = await query.CountAsync(cancellationToken);

        var list = await query
            .OrderByDescending(res => res.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = list.Select(res =>
        {
            decimal totalCharges = res.Folios.Sum(f => f.TotalCharges);
            decimal totalPayments = res.Folios.Sum(f => f.TotalPayments);
            decimal balance = (totalCharges > 0 || totalPayments > 0)
                ? (totalCharges - totalPayments)
                : (res.TotalEstimatedAmount - res.PaidAmount);

            return new ReservationDto(
                res.Id,
                res.ConfirmationNumber,
                res.GuestId,
                res.Guest.FirstName + " " + res.Guest.LastName,
                res.Guest.Email,
                res.Guest.Phone,
                res.Guest.VipStatus,
                res.RoomTypeCode,
                res.AssignedRoomId,
                res.AssignedRoom?.RoomNumber,
                res.CheckInDate.ToString("yyyy-MM-dd"),
                res.CheckOutDate.ToString("yyyy-MM-dd"),
                res.AdultsCount,
                res.ChildrenCount,
                res.ReservationStatus,
                res.BookingSource,
                res.RatePerNight,
                res.TotalEstimatedAmount,
                res.PaidAmount,
                balance,
                res.SpecialRequests,
                res.ActualCheckInAt,
                res.ActualCheckOutAt
            );
        }).ToList();

        return PagedResult<ReservationDto>.Create(dtos, totalCount, page, pageSize);
    }

    public async Task<ReservationDto> CreateReservationAsync(
        CreateReservationRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        if (!DateOnly.TryParse(request.CheckInDate, out var checkIn))
            throw new ArgumentException("Invalid CheckInDate. Expected YYYY-MM-DD.");

        if (!DateOnly.TryParse(request.CheckOutDate, out var checkOut))
            throw new ArgumentException("Invalid CheckOutDate. Expected YYYY-MM-DD.");

        if (checkOut < checkIn)
            throw new ArgumentException("CheckOutDate cannot be earlier than CheckInDate.");

        int nights = Math.Max(1, checkOut.DayNumber - checkIn.DayNumber);

        // Resolve or create guest
        Guest? guest = null;
        if (request.GuestId.HasValue && request.GuestId > 0)
        {
            guest = await _context.Guests.FirstOrDefaultAsync(g => g.Id == request.GuestId.Value, cancellationToken);
        }
        else if (request.NewGuest != null)
        {
            guest = new Guest
            {
                PropertyId = propertyId,
                FirstName = request.NewGuest.FirstName.Trim(),
                LastName = request.NewGuest.LastName.Trim(),
                Email = request.NewGuest.Email.Trim().ToLowerInvariant(),
                Phone = request.NewGuest.Phone.Trim(),
                IdDocumentType = request.NewGuest.IdDocumentType,
                IdDocumentNumber = request.NewGuest.IdDocumentNumber,
                Nationality = request.NewGuest.Nationality,
                VipStatus = string.IsNullOrWhiteSpace(request.NewGuest.VipStatus) ? "Standard" : request.NewGuest.VipStatus.Trim(),
                SpecialPreferences = request.NewGuest.SpecialPreferences,
                Address = request.NewGuest.Address,
                City = request.NewGuest.City,
                Country = request.NewGuest.Country,
                CreatedBy = createdBy
            };
            _context.Guests.Add(guest);
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (guest == null)
            throw new InvalidOperationException("Valid guest must be specified or created.");

        // Calculate rate
        decimal rate = request.CustomRate ?? 150m;
        if (!request.CustomRate.HasValue)
        {
            var defaultRoomOfThisType = await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.PropertyId == propertyId && r.RoomTypeCode == request.RoomTypeCode, cancellationToken);
            if (defaultRoomOfThisType != null && defaultRoomOfThisType.BaseRatePerNight > 0)
            {
                rate = defaultRoomOfThisType.BaseRatePerNight;
            }
        }

        decimal totalEstimated = rate * nights;
        string confirmationNum = $"RES-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        var reservation = new Reservation
        {
            PropertyId = propertyId,
            ConfirmationNumber = confirmationNum,
            GuestId = guest.Id,
            RoomTypeCode = request.RoomTypeCode.Trim().ToUpperInvariant(),
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            AdultsCount = Math.Max(1, request.AdultsCount),
            ChildrenCount = Math.Max(0, request.ChildrenCount),
            ReservationStatus = "CONF",
            BookingSource = string.IsNullOrWhiteSpace(request.BookingSource) ? "DIRECT" : request.BookingSource.Trim().ToUpperInvariant(),
            RatePerNight = rate,
            TotalEstimatedAmount = totalEstimated,
            PaidAmount = 0m,
            SpecialRequests = request.SpecialRequests,
            CreatedBy = createdBy
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);

        // Create default Folio for this reservation
        var folio = new Folio
        {
            PropertyId = propertyId,
            ReservationId = reservation.Id,
            GuestId = guest.Id,
            FolioNumber = $"FOL-{DateTime.UtcNow:yyyyMMdd}-{reservation.Id}",
            Status = "Open",
            TotalCharges = 0m,
            TotalPayments = 0m,
            BalanceAmount = 0m,
            CreatedBy = createdBy
        };
        _context.Folios.Add(folio);
        await _context.SaveChangesAsync(cancellationToken);

        return new ReservationDto(
            reservation.Id,
            reservation.ConfirmationNumber,
            guest.Id,
            guest.FirstName + " " + guest.LastName,
            guest.Email,
            guest.Phone,
            guest.VipStatus,
            reservation.RoomTypeCode,
            null,
            null,
            reservation.CheckInDate.ToString("yyyy-MM-dd"),
            reservation.CheckOutDate.ToString("yyyy-MM-dd"),
            reservation.AdultsCount,
            reservation.ChildrenCount,
            reservation.ReservationStatus,
            reservation.BookingSource,
            reservation.RatePerNight,
            reservation.TotalEstimatedAmount,
            0m,
            reservation.TotalEstimatedAmount,
            reservation.SpecialRequests,
            null,
            null
        );
    }

    public async Task<ReservationDto?> GetReservationByIdAsync(long reservationId, CancellationToken cancellationToken = default)
    {
        var res = await _context.Reservations
            .Include(r => r.Guest)
            .Include(r => r.AssignedRoom)
            .Include(r => r.Folios)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted, cancellationToken);

        if (res == null) return null;

        decimal totalCharges = res.Folios.Sum(f => f.TotalCharges);
        decimal totalPayments = res.Folios.Sum(f => f.TotalPayments);
        decimal balance = (totalCharges > 0 || totalPayments > 0)
            ? (totalCharges - totalPayments)
            : (res.TotalEstimatedAmount - res.PaidAmount);

        return new ReservationDto(
            res.Id,
            res.ConfirmationNumber,
            res.GuestId,
            res.Guest.FirstName + " " + res.Guest.LastName,
            res.Guest.Email,
            res.Guest.Phone,
            res.Guest.VipStatus,
            res.RoomTypeCode,
            res.AssignedRoomId,
            res.AssignedRoom?.RoomNumber,
            res.CheckInDate.ToString("yyyy-MM-dd"),
            res.CheckOutDate.ToString("yyyy-MM-dd"),
            res.AdultsCount,
            res.ChildrenCount,
            res.ReservationStatus,
            res.BookingSource,
            res.RatePerNight,
            res.TotalEstimatedAmount,
            res.PaidAmount,
            balance,
            res.SpecialRequests,
            res.ActualCheckInAt,
            res.ActualCheckOutAt
        );
    }

    #endregion

    #region Front Office Check-in & Check-out

    public async Task<ReservationDto> CheckInGuestAsync(
        CheckInRequest request,
        string checkedInBy,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Guest)
            .Include(r => r.Folios)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken);

        if (reservation == null)
            throw new KeyNotFoundException($"Reservation #{request.ReservationId} not found.");

        if (reservation.ReservationStatus == "CHKI")
            throw new InvalidOperationException("Guest is already checked in.");

        if (reservation.ReservationStatus == "CHKO" || reservation.ReservationStatus == "CANC")
            throw new InvalidOperationException($"Cannot check in reservation with status '{reservation.ReservationStatus}'.");

        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken);
        if (room == null)
            throw new KeyNotFoundException($"Room #{request.RoomId} not found.");

        if (room.RoomStatusCode == "OCC")
            throw new InvalidOperationException($"Room {room.RoomNumber} is currently occupied.");

        if (room.RoomStatusCode == "OOD" || room.RoomStatusCode == "MNT")
            throw new InvalidOperationException($"Room {room.RoomNumber} is currently out of order / under maintenance.");

        // Allocate room & transition status to OCC
        room.RoomStatusCode = "OCC";
        if (!string.IsNullOrWhiteSpace(request.KeyCardUid))
        {
            room.KeyCardUid = request.KeyCardUid.Trim();
        }
        room.ModifiedAt = DateTimeOffset.UtcNow;
        room.ModifiedBy = checkedInBy;

        reservation.AssignedRoomId = room.Id;
        reservation.ReservationStatus = "CHKI";
        reservation.ActualCheckInAt = DateTimeOffset.UtcNow;
        reservation.CheckedInBy = checkedInBy;
        reservation.ModifiedAt = DateTimeOffset.UtcNow;
        reservation.ModifiedBy = checkedInBy;

        // Ensure Folio exists and post initial accommodation room charge
        var folio = reservation.Folios.FirstOrDefault();
        if (folio == null)
        {
            folio = new Folio
            {
                PropertyId = reservation.PropertyId,
                ReservationId = reservation.Id,
                GuestId = reservation.GuestId,
                FolioNumber = $"FOL-{DateTime.UtcNow:yyyyMMdd}-{reservation.Id}",
                Status = "Open",
                CreatedBy = checkedInBy
            };
            _context.Folios.Add(folio);
            await _context.SaveChangesAsync(cancellationToken);
        }

        int nights = Math.Max(1, reservation.CheckOutDate.DayNumber - reservation.CheckInDate.DayNumber);
        decimal roomChargeTotal = reservation.RatePerNight * nights;

        // Add Room Charge line item to Folio
        var roomChargeItem = new FolioItem
        {
            FolioId = folio.Id,
            ItemType = "CHARGE",
            DepartmentCode = "FO",
            Description = $"Room Accommodation ({room.RoomNumber}) - {nights} nights @ ${reservation.RatePerNight:F2}",
            UnitPrice = reservation.RatePerNight,
            Quantity = nights,
            Amount = roomChargeTotal,
            PostedBy = checkedInBy,
            PostedAt = DateTimeOffset.UtcNow
        };
        _context.FolioItems.Add(roomChargeItem);
        folio.TotalCharges += roomChargeTotal;

        // If Advance deposit provided, post payment item
        if (request.AdvanceDeposit.HasValue && request.AdvanceDeposit.Value > 0)
        {
            decimal deposit = request.AdvanceDeposit.Value;
            var paymentItem = new FolioItem
            {
                FolioId = folio.Id,
                ItemType = "PAYMENT",
                DepartmentCode = "FO",
                Description = "Advance Deposit at Check-In",
                UnitPrice = deposit,
                Quantity = 1,
                Amount = deposit,
                PaymentModeCode = request.PaymentModeCode ?? "CC",
                PostedBy = checkedInBy,
                PostedAt = DateTimeOffset.UtcNow
            };
            _context.FolioItems.Add(paymentItem);
            folio.TotalPayments += deposit;
            reservation.PaidAmount += deposit;
        }

        folio.BalanceAmount = folio.TotalCharges - folio.TotalPayments;
        await _context.SaveChangesAsync(cancellationToken);

        // Update Guest total stays
        reservation.Guest.TotalStays += 1;
        await _context.SaveChangesAsync(cancellationToken);

        return new ReservationDto(
            reservation.Id,
            reservation.ConfirmationNumber,
            reservation.GuestId,
            reservation.Guest.FirstName + " " + reservation.Guest.LastName,
            reservation.Guest.Email,
            reservation.Guest.Phone,
            reservation.Guest.VipStatus,
            reservation.RoomTypeCode,
            room.Id,
            room.RoomNumber,
            reservation.CheckInDate.ToString("yyyy-MM-dd"),
            reservation.CheckOutDate.ToString("yyyy-MM-dd"),
            reservation.AdultsCount,
            reservation.ChildrenCount,
            reservation.ReservationStatus,
            reservation.BookingSource,
            reservation.RatePerNight,
            reservation.TotalEstimatedAmount,
            reservation.PaidAmount,
            folio.BalanceAmount,
            reservation.SpecialRequests,
            reservation.ActualCheckInAt,
            reservation.ActualCheckOutAt
        );
    }

    public async Task<ReservationDto> CheckOutGuestAsync(
        CheckOutRequest request,
        string checkedOutBy,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Guest)
            .Include(r => r.AssignedRoom)
            .Include(r => r.Folios)
                .ThenInclude(f => f.Items)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken);

        if (reservation == null)
            throw new KeyNotFoundException($"Reservation #{request.ReservationId} not found.");

        if (reservation.ReservationStatus != "CHKI")
            throw new InvalidOperationException($"Cannot check out reservation with status '{reservation.ReservationStatus}'. Must be Checked In.");

        var folio = reservation.Folios.FirstOrDefault();
        if (folio != null && request.SettlementPayment.HasValue && request.SettlementPayment.Value > 0)
        {
            decimal payment = request.SettlementPayment.Value;
            var paymentItem = new FolioItem
            {
                FolioId = folio.Id,
                ItemType = "PAYMENT",
                DepartmentCode = "FO",
                Description = "Settlement Payment at Check-Out",
                UnitPrice = payment,
                Quantity = 1,
                Amount = payment,
                PaymentModeCode = request.PaymentModeCode ?? "CC",
                PostedBy = checkedOutBy,
                PostedAt = DateTimeOffset.UtcNow
            };
            _context.FolioItems.Add(paymentItem);
            folio.TotalPayments += payment;
            folio.BalanceAmount = folio.TotalCharges - folio.TotalPayments;
            reservation.PaidAmount += payment;
        }

        if (folio != null)
        {
            folio.Status = folio.BalanceAmount <= 0 ? "Settled" : "Closed";
            folio.ClosedAt = DateTimeOffset.UtcNow;
            reservation.Guest.LifetimeSpend += folio.TotalPayments;
        }

        // Automatic Operational State Transition: Assigned room becomes DRT (Dirty / Vacant)!
        if (reservation.AssignedRoom != null)
        {
            reservation.AssignedRoom.RoomStatusCode = "DRT";
            reservation.AssignedRoom.KeyCardUid = null;
            reservation.AssignedRoom.ModifiedAt = DateTimeOffset.UtcNow;
            reservation.AssignedRoom.ModifiedBy = checkedOutBy;
        }

        reservation.ReservationStatus = "CHKO";
        reservation.ActualCheckOutAt = DateTimeOffset.UtcNow;
        reservation.CheckedOutBy = checkedOutOutBy(checkedOutBy);
        reservation.ModifiedAt = DateTimeOffset.UtcNow;
        reservation.ModifiedBy = checkedOutBy;

        await _context.SaveChangesAsync(cancellationToken);

        decimal finalBalance = folio?.BalanceAmount ?? 0m;

        return new ReservationDto(
            reservation.Id,
            reservation.ConfirmationNumber,
            reservation.GuestId,
            reservation.Guest.FirstName + " " + reservation.Guest.LastName,
            reservation.Guest.Email,
            reservation.Guest.Phone,
            reservation.Guest.VipStatus,
            reservation.RoomTypeCode,
            reservation.AssignedRoomId,
            reservation.AssignedRoom?.RoomNumber,
            reservation.CheckInDate.ToString("yyyy-MM-dd"),
            reservation.CheckOutDate.ToString("yyyy-MM-dd"),
            reservation.AdultsCount,
            reservation.ChildrenCount,
            reservation.ReservationStatus,
            reservation.BookingSource,
            reservation.RatePerNight,
            reservation.TotalEstimatedAmount,
            reservation.PaidAmount,
            finalBalance,
            reservation.SpecialRequests,
            reservation.ActualCheckInAt,
            reservation.ActualCheckOutAt
        );
    }

    private static string checkedOutOutBy(string val) => val;

    #endregion

    #region Folios & Billing

    public async Task<FolioDto?> GetFolioByReservationIdAsync(long reservationId, CancellationToken cancellationToken = default)
    {
        var folio = await _context.Folios
            .Include(f => f.Reservation)
            .Include(f => f.Guest)
            .Include(f => f.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.ReservationId == reservationId && !f.IsDeleted, cancellationToken);

        if (folio == null) return null;

        return MapFolioToDto(folio);
    }

    public async Task<FolioDto?> GetFolioByIdAsync(long folioId, CancellationToken cancellationToken = default)
    {
        var folio = await _context.Folios
            .Include(f => f.Reservation)
            .Include(f => f.Guest)
            .Include(f => f.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == folioId && !f.IsDeleted, cancellationToken);

        if (folio == null) return null;

        return MapFolioToDto(folio);
    }

    public async Task<FolioDto> PostChargeAsync(
        PostChargeRequest request,
        string postedBy,
        CancellationToken cancellationToken = default)
    {
        var folio = await _context.Folios
            .Include(f => f.Reservation)
            .Include(f => f.Guest)
            .Include(f => f.Items)
            .FirstOrDefaultAsync(f => f.Id == request.FolioId, cancellationToken);

        if (folio == null)
            throw new KeyNotFoundException($"Folio #{request.FolioId} not found.");

        int qty = Math.Max(1, request.Quantity);
        decimal total = request.UnitPrice * qty;

        var item = new FolioItem
        {
            FolioId = folio.Id,
            ItemType = "CHARGE",
            DepartmentCode = request.DepartmentCode.Trim().ToUpperInvariant(),
            Description = request.Description.Trim(),
            UnitPrice = request.UnitPrice,
            Quantity = qty,
            Amount = total,
            PostedBy = postedBy,
            PostedAt = DateTimeOffset.UtcNow
        };

        _context.FolioItems.Add(item);
        folio.TotalCharges += total;
        folio.BalanceAmount = folio.TotalCharges - folio.TotalPayments;
        folio.ModifiedAt = DateTimeOffset.UtcNow;
        folio.ModifiedBy = postedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return MapFolioToDto(folio);
    }

    public async Task<FolioDto> PostPaymentAsync(
        PostPaymentRequest request,
        string postedBy,
        CancellationToken cancellationToken = default)
    {
        var folio = await _context.Folios
            .Include(f => f.Reservation)
            .Include(f => f.Guest)
            .Include(f => f.Items)
            .FirstOrDefaultAsync(f => f.Id == request.FolioId, cancellationToken);

        if (folio == null)
            throw new KeyNotFoundException($"Folio #{request.FolioId} not found.");

        var item = new FolioItem
        {
            FolioId = folio.Id,
            ItemType = "PAYMENT",
            DepartmentCode = "FO",
            Description = $"Payment ({request.PaymentModeCode})",
            UnitPrice = request.Amount,
            Quantity = 1,
            Amount = request.Amount,
            PaymentModeCode = request.PaymentModeCode.Trim().ToUpperInvariant(),
            TransactionReference = request.TransactionReference?.Trim(),
            PostedBy = postedBy,
            PostedAt = DateTimeOffset.UtcNow
        };

        _context.FolioItems.Add(item);
        folio.TotalPayments += request.Amount;
        folio.BalanceAmount = folio.TotalCharges - folio.TotalPayments;
        if (folio.BalanceAmount <= 0) folio.Status = "Settled";
        folio.ModifiedAt = DateTimeOffset.UtcNow;
        folio.ModifiedBy = postedBy;

        if (folio.Reservation != null)
        {
            folio.Reservation.PaidAmount += request.Amount;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapFolioToDto(folio);
    }

    private static FolioDto MapFolioToDto(Folio folio)
    {
        var itemDtos = folio.Items
            .Where(i => !i.IsDeleted)
            .OrderBy(i => i.PostedAt)
            .Select(i => new FolioItemDto(
                i.Id,
                i.ItemType,
                i.DepartmentCode,
                i.Description,
                i.UnitPrice,
                i.Quantity,
                i.Amount,
                i.PaymentModeCode,
                i.TransactionReference,
                i.PostedAt,
                i.PostedBy
            ))
            .ToList();

        return new FolioDto(
            folio.Id,
            folio.FolioNumber,
            folio.ReservationId,
            folio.Reservation?.ConfirmationNumber ?? string.Empty,
            folio.GuestId,
            folio.Guest != null ? folio.Guest.FirstName + " " + folio.Guest.LastName : string.Empty,
            folio.Status,
            folio.TotalCharges,
            folio.TotalPayments,
            folio.BalanceAmount,
            itemDtos
        );
    }

    #endregion
}

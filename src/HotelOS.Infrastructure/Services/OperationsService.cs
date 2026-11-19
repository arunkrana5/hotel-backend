using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Operations;
using HotelOS.Domain.Operations.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class OperationsService : IOperationsService
{
    private readonly HotelOSDbContext _context;

    public OperationsService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<GuestServiceRequestDto>> GetGuestRequestsPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? department = null,
        string? priority = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.GuestServiceRequests
            .Include(r => r.Room)
            .Include(r => r.Guest)
            .Include(r => r.AssignedToUser)
            .AsNoTracking()
            .Where(r => r.PropertyId == propertyId && !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(r => r.Department == department.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(priority))
            query = query.Where(r => r.Priority == priority.Trim().ToUpperInvariant());

        var totalCount = await query.CountAsync(cancellationToken);

        var requests = await query
            .OrderByDescending(r => r.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new GuestServiceRequestDto(
                r.Id,
                r.RequestNumber,
                r.RoomId,
                r.Room != null ? r.Room.RoomNumber : string.Empty,
                r.Room != null ? r.Room.FloorNumber : 0,
                r.GuestId,
                r.Guest != null ? $"{r.Guest.FirstName} {r.Guest.LastName}" : null,
                r.RequestType,
                r.Department,
                r.Priority,
                r.Status,
                r.Description,
                r.AssignedToUserId,
                r.AssignedToUser != null ? r.AssignedToUser.FullName : null,
                r.RequestedAt,
                r.ResolvedAt,
                r.ResolutionNotes
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<GuestServiceRequestDto>
        {
            Items = requests,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<GuestServiceRequestDto> CreateGuestRequestAsync(
        CreateGuestServiceRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms.FindAsync([request.RoomId], cancellationToken)
            ?? throw new KeyNotFoundException($"Room with ID {request.RoomId} not found.");

        long? guestId = request.GuestId;
        if (!guestId.HasValue)
        {
            // Auto-detect currently in-house guest in this room
            var activeRes = await _context.Reservations
                .Where(r => r.AssignedRoomId == request.RoomId && r.ReservationStatus == "CHKI" && !r.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            guestId = activeRes?.GuestId;
        }

        var countToday = await _context.GuestServiceRequests
            .CountAsync(r => r.PropertyId == propertyId && r.RequestedAt.Date == DateTimeOffset.UtcNow.Date, cancellationToken);

        var requestNumber = $"REQ-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";

        var serviceReq = new GuestServiceRequest
        {
            PropertyId = propertyId,
            RequestNumber = requestNumber,
            RoomId = request.RoomId,
            GuestId = guestId,
            RequestType = request.RequestType.Trim().ToUpperInvariant(),
            Department = request.Department.Trim().ToUpperInvariant(),
            Priority = request.Priority.Trim().ToUpperInvariant(),
            Status = request.AssignedToUserId.HasValue ? "ASSIGNED" : "PENDING",
            Description = request.Description,
            AssignedToUserId = request.AssignedToUserId,
            RequestedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.GuestServiceRequests.Add(serviceReq);
        await _context.SaveChangesAsync(cancellationToken);

        string? guestName = null;
        if (serviceReq.GuestId.HasValue)
        {
            var guest = await _context.Guests.FindAsync([serviceReq.GuestId.Value], cancellationToken);
            guestName = guest != null ? $"{guest.FirstName} {guest.LastName}" : null;
        }

        string? assignedName = null;
        if (serviceReq.AssignedToUserId.HasValue)
        {
            var user = await _context.Users.FindAsync([serviceReq.AssignedToUserId.Value], cancellationToken);
            assignedName = user?.FullName;
        }

        return new GuestServiceRequestDto(
            serviceReq.Id,
            serviceReq.RequestNumber,
            serviceReq.RoomId,
            room.RoomNumber,
            room.FloorNumber,
            serviceReq.GuestId,
            guestName,
            serviceReq.RequestType,
            serviceReq.Department,
            serviceReq.Priority,
            serviceReq.Status,
            serviceReq.Description,
            serviceReq.AssignedToUserId,
            assignedName,
            serviceReq.RequestedAt,
            serviceReq.ResolvedAt,
            serviceReq.ResolutionNotes
        );
    }

    public async Task<GuestServiceRequestDto?> UpdateGuestRequestStatusAsync(
        long requestId,
        UpdateGuestServiceRequestStatus request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var serviceReq = await _context.GuestServiceRequests
            .Include(r => r.Room)
            .Include(r => r.Guest)
            .Include(r => r.AssignedToUser)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (serviceReq == null) return null;

        var newStatus = request.Status.Trim().ToUpperInvariant();
        serviceReq.Status = newStatus;
        serviceReq.ModifiedAt = DateTimeOffset.UtcNow;
        serviceReq.ModifiedBy = updatedBy;

        if (request.AssignedToUserId.HasValue)
        {
            serviceReq.AssignedToUserId = request.AssignedToUserId.Value;
        }

        if (request.ResolutionNotes != null)
        {
            serviceReq.ResolutionNotes = request.ResolutionNotes;
        }

        if (newStatus == "RESOLVED" || newStatus == "CLOSED")
        {
            serviceReq.ResolvedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new GuestServiceRequestDto(
            serviceReq.Id,
            serviceReq.RequestNumber,
            serviceReq.RoomId,
            serviceReq.Room != null ? serviceReq.Room.RoomNumber : string.Empty,
            serviceReq.Room != null ? serviceReq.Room.FloorNumber : 0,
            serviceReq.GuestId,
            serviceReq.Guest != null ? $"{serviceReq.Guest.FirstName} {serviceReq.Guest.LastName}" : null,
            serviceReq.RequestType,
            serviceReq.Department,
            serviceReq.Priority,
            serviceReq.Status,
            serviceReq.Description,
            serviceReq.AssignedToUserId,
            serviceReq.AssignedToUser?.FullName,
            serviceReq.RequestedAt,
            serviceReq.ResolvedAt,
            serviceReq.ResolutionNotes
        );
    }
}

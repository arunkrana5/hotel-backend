using System.Security.Cryptography;
using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Operations;
using HotelOS.Contracts.Restaurant;
using HotelOS.Domain.Operations.Entities;
using HotelOS.Domain.Pms.Entities;
using HotelOS.Domain.Restaurant.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HotelOS.Infrastructure.Services;

public class GuestOrderService : IGuestOrderService
{
    private readonly HotelOSDbContext _context;
    private readonly string _guestBaseUrl;

    public GuestOrderService(HotelOSDbContext _context, IConfiguration configuration)
    {
        this._context = _context;
        var clientUrl = configuration["GuestPortal:BaseUrl"]
            ?? (!string.IsNullOrWhiteSpace(configuration["App:ClientUrl"]) ? $"{configuration["App:ClientUrl"]!.TrimEnd('/')}/guest/q" : null)
            ?? "https://hotal-dho2.onrender.com/guest/q";
        _guestBaseUrl = clientUrl;
    }

    public async Task<List<RoomQrCodeDto>> GetRoomQrCodesAsync(
        long propertyId = 1,
        CancellationToken cancellationToken = default)
    {
        var qrCodes = await _context.RoomQrCodes
            .Include(q => q.Room)
            .AsNoTracking()
            .Where(q => q.PropertyId == propertyId && !q.IsDeleted)
            .OrderBy(q => q.Room != null ? q.Room.RoomNumber : string.Empty)
            .ToListAsync(cancellationToken);

        return qrCodes.Select(q => new RoomQrCodeDto(
            q.Id,
            q.PropertyId,
            q.RoomId,
            q.Room?.RoomNumber ?? $"Room-{q.RoomId}",
            q.Room?.RoomTypeCode ?? "STD",
            q.QrToken,
            $"{_guestBaseUrl.TrimEnd('/')}/{q.QrToken}",
            q.RequirePin,
            q.PinCode,
            q.IsActive,
            q.ScanCount,
            q.GeneratedAt,
            q.LastScannedAt
        )).ToList();
    }

    public async Task<RoomQrCodeDto> GenerateRoomQrAsync(
        long propertyId,
        GenerateRoomQrRequest request,
        long? userId,
        CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.PropertyId == propertyId && !r.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"Room with ID {request.RoomId} not found in property {propertyId}.");

        var existing = await _context.RoomQrCodes
            .FirstOrDefaultAsync(q => q.RoomId == request.RoomId && q.PropertyId == propertyId && !q.IsDeleted, cancellationToken);

        var token = GenerateSecureToken();

        if (existing != null)
        {
            existing.QrToken = token;
            existing.RequirePin = request.RequirePin;
            existing.PinCode = request.PinCode;
            existing.IsActive = true;
            existing.GeneratedAt = DateTimeOffset.UtcNow;
            existing.RevokedAt = null;
            existing.GeneratedByUserId = userId;
            existing.ModifiedAt = DateTimeOffset.UtcNow;
            existing.ModifiedBy = userId?.ToString() ?? "STAFF";

            await _context.SaveChangesAsync(cancellationToken);

            return new RoomQrCodeDto(
                existing.Id,
                existing.PropertyId,
                existing.RoomId,
                room.RoomNumber,
                room.RoomTypeCode,
                existing.QrToken,
                $"{_guestBaseUrl.TrimEnd('/')}/{existing.QrToken}",
                existing.RequirePin,
                existing.PinCode,
                existing.IsActive,
                existing.ScanCount,
                existing.GeneratedAt,
                existing.LastScannedAt
            );
        }

        var qrCode = new RoomQrCode
        {
            PropertyId = propertyId,
            RoomId = room.Id,
            QrToken = token,
            RequirePin = request.RequirePin,
            PinCode = request.PinCode,
            IsActive = true,
            GeneratedAt = DateTimeOffset.UtcNow,
            GeneratedByUserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId?.ToString() ?? "STAFF"
        };

        _context.RoomQrCodes.Add(qrCode);
        await _context.SaveChangesAsync(cancellationToken);

        return new RoomQrCodeDto(
            qrCode.Id,
            qrCode.PropertyId,
            qrCode.RoomId,
            room.RoomNumber,
            room.RoomTypeCode,
            qrCode.QrToken,
            $"{_guestBaseUrl.TrimEnd('/')}/{qrCode.QrToken}",
            qrCode.RequirePin,
            qrCode.PinCode,
            qrCode.IsActive,
            qrCode.ScanCount,
            qrCode.GeneratedAt,
            qrCode.LastScannedAt
        );
    }

    public async Task<List<RoomQrCodeDto>> BulkGenerateRoomQrAsync(
        long propertyId,
        BulkGenerateRoomQrRequest request,
        long? userId,
        CancellationToken cancellationToken = default)
    {
        var roomQuery = _context.Rooms
            .Where(r => r.PropertyId == propertyId && !r.IsDeleted && r.IsActive);

        if (request.RoomIds != null && request.RoomIds.Count > 0)
        {
            roomQuery = roomQuery.Where(r => request.RoomIds.Contains(r.Id));
        }

        var rooms = await roomQuery.ToListAsync(cancellationToken);
        var existingQrs = await _context.RoomQrCodes
            .Where(q => q.PropertyId == propertyId && !q.IsDeleted)
            .ToDictionaryAsync(q => q.RoomId, cancellationToken);

        var result = new List<RoomQrCodeDto>();

        foreach (var room in rooms)
        {
            if (existingQrs.TryGetValue(room.Id, out var existing))
            {
                if (request.OverwriteExisting)
                {
                    existing.QrToken = GenerateSecureToken();
                    existing.IsActive = true;
                    existing.GeneratedAt = DateTimeOffset.UtcNow;
                    existing.RevokedAt = null;
                    existing.GeneratedByUserId = userId;
                    existing.ModifiedAt = DateTimeOffset.UtcNow;
                    existing.ModifiedBy = userId?.ToString() ?? "STAFF";

                    result.Add(new RoomQrCodeDto(
                        existing.Id,
                        existing.PropertyId,
                        existing.RoomId,
                        room.RoomNumber,
                        room.RoomTypeCode,
                        existing.QrToken,
                        $"{_guestBaseUrl.TrimEnd('/')}/{existing.QrToken}",
                        existing.RequirePin,
                        existing.PinCode,
                        existing.IsActive,
                        existing.ScanCount,
                        existing.GeneratedAt,
                        existing.LastScannedAt
                    ));
                }
                else
                {
                    result.Add(new RoomQrCodeDto(
                        existing.Id,
                        existing.PropertyId,
                        existing.RoomId,
                        room.RoomNumber,
                        room.RoomTypeCode,
                        existing.QrToken,
                        $"{_guestBaseUrl.TrimEnd('/')}/{existing.QrToken}",
                        existing.RequirePin,
                        existing.PinCode,
                        existing.IsActive,
                        existing.ScanCount,
                        existing.GeneratedAt,
                        existing.LastScannedAt
                    ));
                }
            }
            else
            {
                var token = GenerateSecureToken();
                var newQr = new RoomQrCode
                {
                    PropertyId = propertyId,
                    RoomId = room.Id,
                    QrToken = token,
                    RequirePin = false,
                    IsActive = true,
                    GeneratedAt = DateTimeOffset.UtcNow,
                    GeneratedByUserId = userId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = userId?.ToString() ?? "STAFF"
                };

                _context.RoomQrCodes.Add(newQr);
                result.Add(new RoomQrCodeDto(
                    0, // will be generated upon save
                    propertyId,
                    room.Id,
                    room.RoomNumber,
                    room.RoomTypeCode,
                    newQr.QrToken,
                    $"{_guestBaseUrl.TrimEnd('/')}/{newQr.QrToken}",
                    newQr.RequirePin,
                    newQr.PinCode,
                    newQr.IsActive,
                    newQr.ScanCount,
                    newQr.GeneratedAt,
                    newQr.LastScannedAt
                ));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetRoomQrCodesAsync(propertyId, cancellationToken);
    }

    public async Task<bool> RevokeRoomQrAsync(long qrCodeId, CancellationToken cancellationToken = default)
    {
        var qr = await _context.RoomQrCodes.FindAsync([qrCodeId], cancellationToken);
        if (qr == null) return false;

        qr.IsActive = false;
        qr.RevokedAt = DateTimeOffset.UtcNow;
        qr.ModifiedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PagedResult<RestaurantOrderDto>> GetRoomOrdersPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 15,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.RestaurantOrders
            .Include(o => o.Room)
            .Include(o => o.Guest)
            .Include(o => o.ServerUser)
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.PropertyId == propertyId && o.OrderType == "ROOM_SERVICE" && !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var st = status.Trim().ToUpperInvariant();
            if (st == "NEW")
            {
                query = query.Where(o => o.Status == "PLACED");
            }
            else
            {
                query = query.Where(o => o.Status == st);
            }
        }

        var total = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.PlacedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RestaurantOrderDto>
        {
            Items = orders.Select(o => new RestaurantOrderDto(
                o.Id,
                o.OrderNumber,
                o.OrderType,
                o.TableId,
                null,
                o.RoomId,
                o.Room?.RoomNumber,
                o.GuestId,
                o.Guest != null ? $"{o.Guest.FirstName} {o.Guest.LastName}" : o.GuestDisplayName,
                o.FolioId,
                o.Status,
                o.SubTotal,
                o.TaxAmount,
                o.DiscountAmount,
                o.TotalAmount,
                o.PaymentStatus,
                o.PaymentMethod,
                o.ServerUserId,
                o.ServerUser?.FullName ?? o.DeliveryStaffName,
                o.SpecialInstructions,
                o.PlacedAt,
                o.PreparedAt,
                o.ServedAt,
                o.Items.Select(i => new RestaurantOrderItemDto(
                    i.Id,
                    i.MenuItemId,
                    i.ItemName,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice,
                    i.Status,
                    i.Notes
                )).ToList(),
                o.DeliveredAt,
                o.Source,
                o.DeliveryContactPhone,
                o.GuestDisplayName,
                o.IsStayVerified,
                o.EstimatedDeliveryMinutes,
                o.DeliveryStaffName
            )).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<RestaurantOrderDto?> UpdateRoomOrderStatusAsync(
        long orderId,
        StaffUpdateRoomOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Room)
            .Include(o => o.Guest)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null) return null;

        var targetStatus = request.Status.Trim().ToUpperInvariant();
        order.Status = targetStatus;
        order.ModifiedAt = DateTimeOffset.UtcNow;
        order.ModifiedBy = updatedBy;

        if (request.EstimatedDeliveryMinutes.HasValue)
        {
            order.EstimatedDeliveryMinutes = request.EstimatedDeliveryMinutes.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.DeliveryStaffName))
        {
            order.DeliveryStaffName = request.DeliveryStaffName.Trim();
        }

        switch (targetStatus)
        {
            case "ACCEPTED":
                order.AcceptedAt = DateTimeOffset.UtcNow;
                if (!order.EstimatedDeliveryMinutes.HasValue)
                {
                    order.EstimatedDeliveryMinutes = 30;
                }
                break;

            case "COOKING":
                foreach (var item in order.Items.Where(i => i.Status == "QUEUED"))
                {
                    item.Status = "PREPARING";
                    item.ModifiedAt = DateTimeOffset.UtcNow;
                    item.ModifiedBy = updatedBy;
                }
                break;

            case "READY":
                order.PreparedAt = DateTimeOffset.UtcNow;
                foreach (var item in order.Items.Where(i => i.Status == "PREPARING" || i.Status == "QUEUED"))
                {
                    item.Status = "READY";
                    item.ModifiedAt = DateTimeOffset.UtcNow;
                    item.ModifiedBy = updatedBy;
                }
                break;

            case "OUT_FOR_DELIVERY":
                // Delivery staff on the way
                break;

            case "DELIVERED":
            case "SERVED":
                order.DeliveredAt = DateTimeOffset.UtcNow;
                order.ServedAt = DateTimeOffset.UtcNow;
                order.Status = "DELIVERED";
                foreach (var item in order.Items)
                {
                    item.Status = "SERVED";
                    item.ModifiedAt = DateTimeOffset.UtcNow;
                    item.ModifiedBy = updatedBy;
                }

                // If payment method is CHARGE_TO_ROOM and FolioItem not yet recorded, record it now
                if (order.PaymentStatus == "CHARGED_TO_ROOM" && order.FolioId.HasValue)
                {
                    var existingCharge = await _context.FolioItems
                        .AnyAsync(f => f.FolioId == order.FolioId.Value && f.TransactionReference == order.OrderNumber, cancellationToken);

                    if (!existingCharge)
                    {
                        var folioCharge = new FolioItem
                        {
                            FolioId = order.FolioId.Value,
                            ItemType = "CHARGE",
                            DepartmentCode = "FB",
                            Description = $"In-Room Service Order #{order.OrderNumber} (Room {order.Room?.RoomNumber})",
                            UnitPrice = order.TotalAmount,
                            Quantity = 1,
                            Amount = order.TotalAmount,
                            TransactionReference = order.OrderNumber,
                            PostedAt = DateTimeOffset.UtcNow,
                            PostedBy = updatedBy,
                            CreatedAt = DateTimeOffset.UtcNow,
                            CreatedBy = updatedBy
                        };
                        _context.FolioItems.Add(folioCharge);
                    }
                }
                break;

            case "CANCELLED":
                order.Status = "CANCELLED";
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new RestaurantOrderDto(
            order.Id,
            order.OrderNumber,
            order.OrderType,
            order.TableId,
            null,
            order.RoomId,
            order.Room?.RoomNumber,
            order.GuestId,
            order.Guest != null ? $"{order.Guest.FirstName} {order.Guest.LastName}" : order.GuestDisplayName,
            order.FolioId,
            order.Status,
            order.SubTotal,
            order.TaxAmount,
            order.DiscountAmount,
            order.TotalAmount,
            order.PaymentStatus,
            order.PaymentMethod,
            order.ServerUserId,
            order.DeliveryStaffName,
            order.SpecialInstructions,
            order.PlacedAt,
            order.PreparedAt,
            order.ServedAt,
            order.Items.Select(i => new RestaurantOrderItemDto(
                i.Id,
                i.MenuItemId,
                i.ItemName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Status,
                i.Notes
            )).ToList(),
            order.DeliveredAt,
            order.Source,
            order.DeliveryContactPhone,
            order.GuestDisplayName,
            order.IsStayVerified,
            order.EstimatedDeliveryMinutes,
            order.DeliveryStaffName
        );
    }

    public async Task<QrValidationResultDto> ValidateQrTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new QrValidationResultDto(
                false, "INVALID_TOKEN", "QR code token is missing or invalid.",
                null, null, null, null, null, false, false, null, null
            );
        }

        var cleanToken = token.Trim();
        var qrCode = await _context.RoomQrCodes
            .Include(q => q.Room)
            .FirstOrDefaultAsync(q => q.QrToken == cleanToken && !q.IsDeleted, cancellationToken);

        if (qrCode == null)
        {
            return new QrValidationResultDto(
                false, "NOT_FOUND", "The scanned QR code is unrecognized or not configured.",
                null, null, null, null, null, false, false, null, null
            );
        }

        if (!qrCode.IsActive)
        {
            return new QrValidationResultDto(
                false, "REVOKED", "This QR code has been disabled by hotel management.",
                null, null, null, null, null, false, false, null, null
            );
        }

        // Increment scan statistics
        qrCode.ScanCount++;
        qrCode.LastScannedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var room = qrCode.Room;
        if (room == null || !room.IsActive || room.IsDeleted)
        {
            return new QrValidationResultDto(
                false, "ROOM_INACTIVE", "The room associated with this QR code is currently inactive.",
                null, null, null, null, null, false, false, null, null
            );
        }

        // Check active stay
        var activeRes = await _context.Reservations
            .Include(r => r.Guest)
            .FirstOrDefaultAsync(r => r.AssignedRoomId == room.Id && r.ReservationStatus == "CHKI" && !r.IsDeleted, cancellationToken);

        string? guestName = null;
        if (activeRes?.Guest != null)
        {
            guestName = $"{activeRes.Guest.FirstName} {activeRes.Guest.LastName}".Trim();
        }

        return new QrValidationResultDto(
            IsValid: true,
            ErrorCode: null,
            ErrorMessage: null,
            PropertyId: qrCode.PropertyId,
            PropertyName: "Grand Hotel & Resort",
            RoomId: room.Id,
            RoomNumber: room.RoomNumber,
            RoomType: room.RoomTypeCode,
            RequirePin: qrCode.RequirePin,
            IsOccupied: activeRes != null,
            GuestName: guestName,
            SessionToken: null
        );
    }

    public async Task<GuestSessionDto> InitializeSessionAsync(
        GuestSessionInitRequest request,
        CancellationToken cancellationToken = default)
    {
        var cleanToken = request.QrToken?.Trim() ?? string.Empty;
        var qrCode = await _context.RoomQrCodes
            .Include(q => q.Room)
            .FirstOrDefaultAsync(q => q.QrToken == cleanToken && q.IsActive && !q.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException("Invalid or inactive QR code token.");

        if (qrCode.RequirePin)
        {
            if (string.IsNullOrWhiteSpace(request.PinCode) || request.PinCode.Trim() != qrCode.PinCode)
            {
                throw new UnauthorizedAccessException("Incorrect room security PIN.");
            }
        }

        var room = qrCode.Room!;

        // Check active stay
        var activeRes = await _context.Reservations
            .Include(r => r.Guest)
            .FirstOrDefaultAsync(r => r.AssignedRoomId == room.Id && r.ReservationStatus == "CHKI" && !r.IsDeleted, cancellationToken);

        var isStayVerified = activeRes != null;
        var guestId = activeRes?.GuestId;
        var guestName = !string.IsNullOrWhiteSpace(request.GuestName)
            ? request.GuestName.Trim()
            : (activeRes?.Guest != null ? $"{activeRes.Guest.FirstName} {activeRes.Guest.LastName}".Trim() : $"Room {room.RoomNumber} Guest");

        var sessionToken = GenerateSecureToken();
        var session = new GuestOrderingSession
        {
            PropertyId = qrCode.PropertyId,
            RoomId = room.Id,
            RoomQrCodeId = qrCode.Id,
            SessionToken = sessionToken,
            ReservationId = activeRes?.Id,
            GuestId = guestId,
            GuestName = guestName,
            GuestPhone = request.GuestPhone?.Trim(),
            IsStayVerified = isStayVerified,
            VerificationMode = isStayVerified ? "AUTO" : (!string.IsNullOrWhiteSpace(request.GuestPhone) ? "PHONE" : "UNVERIFIED"),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
            LastActivityAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "GUEST_PORTAL"
        };

        _context.GuestOrderingSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return new GuestSessionDto(
            sessionToken,
            qrCode.PropertyId,
            "Grand Hotel & Resort",
            room.Id,
            room.RoomNumber,
            guestName,
            isStayVerified,
            session.VerificationMode,
            session.ExpiresAt
        );
    }

    public async Task<List<MenuItemDto>> GetGuestMenuAsync(
        string sessionToken,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.GuestOrderingSessions
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive && !s.IsDeleted, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid or expired session. Please re-scan room QR code.");

        if (session.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedAccessException("Session expired. Please re-scan room QR code.");
        }

        session.LastActivityAt = DateTimeOffset.UtcNow;

        var query = _context.MenuItems
            .AsNoTracking()
            .Where(m => m.PropertyId == session.PropertyId && m.IsActive && !m.IsDeleted && m.IsAvailable);

        if (!string.IsNullOrWhiteSpace(category) && category.Trim().ToUpperInvariant() != "ALL")
        {
            query = query.Where(m => m.Category == category.Trim().ToUpperInvariant());
        }

        var items = await query
            .OrderBy(m => m.Category)
            .ThenBy(m => m.ItemName)
            .ToListAsync(cancellationToken);

        return items.Select(m => new MenuItemDto(
            m.Id,
            m.ItemCode,
            m.ItemName,
            m.Category,
            m.Description,
            m.Price,
            m.PreparationTimeMinutes,
            m.DietaryType,
            m.IsAvailable,
            m.ImageUrl
        )).ToList();
    }

    public async Task<GuestOrderSummaryDto> PlaceGuestOrderAsync(
        GuestPlaceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one item.");
        }

        var session = await _context.GuestOrderingSessions
            .Include(s => s.Room)
            .Include(s => s.Reservation)
            .FirstOrDefaultAsync(s => s.SessionToken == request.SessionToken && s.IsActive && !s.IsDeleted, cancellationToken)
            ?? throw new UnauthorizedAccessException("Session invalid or expired. Please re-scan room QR code.");

        if (session.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedAccessException("Session has expired. Please re-scan room QR code.");
        }

        session.LastActivityAt = DateTimeOffset.UtcNow;

        // Recalculate price server-side
        var itemIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();
        var menuItems = await _context.MenuItems
            .Where(m => itemIds.Contains(m.Id) && m.PropertyId == session.PropertyId && m.IsActive && !m.IsDeleted)
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        decimal subtotal = 0;
        var orderItems = new List<RestaurantOrderItem>();

        foreach (var reqItem in request.Items)
        {
            if (!menuItems.TryGetValue(reqItem.MenuItemId, out var menuItem))
            {
                throw new KeyNotFoundException($"Menu item with ID {reqItem.MenuItemId} was not found or is unavailable.");
            }

            if (!menuItem.IsAvailable)
            {
                throw new InvalidOperationException($"'{menuItem.ItemName}' is currently out of stock.");
            }

            var lineTotal = reqItem.Quantity * menuItem.Price;
            subtotal += lineTotal;

            orderItems.Add(new RestaurantOrderItem
            {
                MenuItemId = menuItem.Id,
                ItemName = menuItem.ItemName,
                Quantity = reqItem.Quantity,
                UnitPrice = menuItem.Price,
                TotalPrice = lineTotal,
                Status = "QUEUED",
                Notes = reqItem.Notes?.Trim(),
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "GUEST_QR"
            });
        }

        var taxAmount = Math.Round(subtotal * 0.05m, 2); // 5% VAT / Service tax
        var totalAmount = subtotal + taxAmount;

        // Generate Order Number
        var today = DateTimeOffset.UtcNow.Date;
        var countToday = await _context.RestaurantOrders
            .CountAsync(o => o.PropertyId == session.PropertyId && o.PlacedAt.Date == today, cancellationToken);
        var orderNumber = $"RS-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";

        // Room folio resolution
        long? folioId = null;
        var paymentMethod = request.PaymentMethod?.Trim().ToUpperInvariant() ?? "PAY_ON_DELIVERY";
        string paymentStatus = "UNPAID";

        if (paymentMethod == "CHARGE_TO_ROOM")
        {
            var activeRes = await _context.Reservations
                .Include(r => r.Folios)
                .FirstOrDefaultAsync(r => r.AssignedRoomId == session.RoomId && r.ReservationStatus == "CHKI" && !r.IsDeleted, cancellationToken);

            var masterFolio = activeRes?.Folios.FirstOrDefault(f => f.Status == "Open");
            if (masterFolio != null)
            {
                folioId = masterFolio.Id;
                paymentStatus = "CHARGED_TO_ROOM";

                var folioItem = new FolioItem
                {
                    FolioId = masterFolio.Id,
                    ItemType = "CHARGE",
                    DepartmentCode = "FB",
                    Description = $"In-Room Service Order #{orderNumber} (Room {session.Room?.RoomNumber})",
                    UnitPrice = totalAmount,
                    Quantity = 1,
                    Amount = totalAmount,
                    TransactionReference = orderNumber,
                    PostedAt = DateTimeOffset.UtcNow,
                    PostedBy = "GUEST_QR",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "GUEST_QR"
                };
                _context.FolioItems.Add(folioItem);
            }
            else
            {
                // Fallback to Pay on Delivery if no open folio
                paymentMethod = "PAY_ON_DELIVERY";
                paymentStatus = "UNPAID";
            }
        }

        var order = new RestaurantOrder
        {
            PropertyId = session.PropertyId,
            OrderNumber = orderNumber,
            OrderType = "ROOM_SERVICE",
            Source = "GUEST_QR",
            RoomId = session.RoomId,
            GuestId = session.GuestId,
            FolioId = folioId,
            GuestDisplayName = !string.IsNullOrWhiteSpace(request.GuestDisplayName) ? request.GuestDisplayName.Trim() : session.GuestName,
            DeliveryContactPhone = !string.IsNullOrWhiteSpace(request.DeliveryContactPhone) ? request.DeliveryContactPhone.Trim() : session.GuestPhone,
            IsStayVerified = session.IsStayVerified,
            GuestOrderingSessionId = session.Id,
            Status = "PLACED",
            SubTotal = subtotal,
            TaxAmount = taxAmount,
            DiscountAmount = 0,
            TotalAmount = totalAmount,
            PaymentStatus = paymentStatus,
            PaymentMethod = paymentMethod,
            SpecialInstructions = request.SpecialInstructions?.Trim(),
            PlacedAt = DateTimeOffset.UtcNow,
            EstimatedDeliveryMinutes = 35,
            Items = orderItems,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "GUEST_PORTAL"
        };

        _context.RestaurantOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return new GuestOrderSummaryDto(
            order.Id,
            order.OrderNumber,
            session.RoomId,
            session.Room?.RoomNumber ?? $"Room-{session.RoomId}",
            order.GuestDisplayName,
            order.DeliveryContactPhone,
            order.IsStayVerified,
            order.Status,
            order.SubTotal,
            order.TaxAmount,
            order.TotalAmount,
            order.PaymentStatus,
            order.PaymentMethod,
            order.SpecialInstructions,
            order.PlacedAt,
            order.AcceptedAt,
            order.PreparedAt,
            order.DeliveredAt,
            order.EstimatedDeliveryMinutes,
            order.DeliveryStaffName,
            order.Items.Select(i => new RestaurantOrderItemDto(
                i.Id,
                i.MenuItemId,
                i.ItemName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Status,
                i.Notes
            )).ToList()
        );
    }

    public async Task<GuestOrderSummaryDto?> GetGuestOrderDetailsAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Room)
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber.Trim(), cancellationToken);

        if (order == null) return null;

        return new GuestOrderSummaryDto(
            order.Id,
            order.OrderNumber,
            order.RoomId ?? 0,
            order.Room?.RoomNumber ?? "Room",
            order.GuestDisplayName,
            order.DeliveryContactPhone,
            order.IsStayVerified,
            order.Status,
            order.SubTotal,
            order.TaxAmount,
            order.TotalAmount,
            order.PaymentStatus,
            order.PaymentMethod,
            order.SpecialInstructions,
            order.PlacedAt,
            order.AcceptedAt,
            order.PreparedAt,
            order.DeliveredAt,
            order.EstimatedDeliveryMinutes,
            order.DeliveryStaffName,
            order.Items.Select(i => new RestaurantOrderItemDto(
                i.Id,
                i.MenuItemId,
                i.ItemName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Status,
                i.Notes
            )).ToList()
        );
    }

    public async Task<GuestServiceRequestDto> RequestServiceAsync(
        GuestCreateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.GuestOrderingSessions
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.SessionToken == request.SessionToken && s.IsActive && !s.IsDeleted, cancellationToken)
            ?? throw new UnauthorizedAccessException("Session invalid or expired.");

        var reqType = request.RequestType.Trim().ToUpperInvariant();
        var department = "HOUSEKEEPING";
        var priority = "MEDIUM";

        if (reqType.Contains("MAINTENANCE") || reqType.Contains("AC") || reqType.Contains("TV") || reqType.Contains("PLUMBING"))
        {
            department = "MAINTENANCE";
        }
        else if (reqType.Contains("WATER") || reqType.Contains("FOOD") || reqType.Contains("BEVERAGE"))
        {
            department = "RESTAURANT";
        }
        else if (reqType.Contains("LUGGAGE") || reqType.Contains("BELLBOY") || reqType.Contains("WAKEUP"))
        {
            department = "FRONT_OFFICE";
        }

        var today = DateTimeOffset.UtcNow.Date;
        var countToday = await _context.GuestServiceRequests
            .CountAsync(r => r.PropertyId == session.PropertyId && r.RequestedAt.Date == today, cancellationToken);
        var reqNumber = $"SR-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";

        var serviceReq = new GuestServiceRequest
        {
            PropertyId = session.PropertyId,
            RequestNumber = reqNumber,
            RoomId = session.RoomId,
            GuestId = session.GuestId,
            RequestType = reqType,
            Department = department,
            Priority = priority,
            Status = "PENDING",
            Description = request.Description?.Trim() ?? $"Guest requested {reqType.Replace('_', ' ')}",
            RequestedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "GUEST_PORTAL"
        };

        _context.GuestServiceRequests.Add(serviceReq);
        await _context.SaveChangesAsync(cancellationToken);

        return new GuestServiceRequestDto(
            serviceReq.Id,
            serviceReq.RequestNumber,
            serviceReq.RoomId,
            session.Room?.RoomNumber ?? $"Room-{session.RoomId}",
            session.Room?.FloorNumber ?? 0,
            session.GuestId,
            session.GuestName,
            serviceReq.RequestType,
            serviceReq.Department,
            serviceReq.Priority,
            serviceReq.Status,
            serviceReq.Description,
            null,
            null,
            serviceReq.RequestedAt,
            null,
            null
        );
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(20);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

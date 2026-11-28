namespace HotelOS.Domain.Restaurant.Entities;

using HotelOS.Domain.Common;
using HotelOS.Domain.Pms.Entities;

public class GuestOrderingSession : BaseEntity
{
    public long PropertyId { get; set; }
    public long RoomId { get; set; }
    public Room? Room { get; set; }
    public long RoomQrCodeId { get; set; }
    public RoomQrCode? RoomQrCode { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public long? ReservationId { get; set; }
    public Reservation? Reservation { get; set; }
    public long? GuestId { get; set; }
    public Guest? Guest { get; set; }
    public string? GuestName { get; set; }
    public string? GuestPhone { get; set; }
    public bool IsStayVerified { get; set; }
    public string VerificationMode { get; set; } = "AUTO"; // AUTO, PHONE, PIN, UNVERIFIED
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
}

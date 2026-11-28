namespace HotelOS.Domain.Restaurant.Entities;

using HotelOS.Domain.Common;
using HotelOS.Domain.Pms.Entities;

public class RoomQrCode : BaseEntity
{
    public long PropertyId { get; set; }
    public long RoomId { get; set; }
    public Room? Room { get; set; }
    public string QrToken { get; set; } = string.Empty;
    public string? PinCode { get; set; }
    public bool RequirePin { get; set; } = false;
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; set; }
    public long? GeneratedByUserId { get; set; }
    public int ScanCount { get; set; } = 0;
    public DateTimeOffset? LastScannedAt { get; set; }
}

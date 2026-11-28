namespace HotelOS.Domain.Restaurant.Entities;

using HotelOS.Domain.Common;
using HotelOS.Domain.Identity.Entities;
using HotelOS.Domain.Pms.Entities;

public class RestaurantOrder : BaseEntity
{
    public long PropertyId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderType { get; set; } = "DINE_IN"; // DINE_IN, ROOM_SERVICE, TAKEAWAY
    public long? TableId { get; set; }
    public DiningTable? Table { get; set; }
    public long? RoomId { get; set; }
    public Room? Room { get; set; }
    public long? GuestId { get; set; }
    public Guest? Guest { get; set; }
    public long? FolioId { get; set; }
    public Folio? Folio { get; set; }
    public string Status { get; set; } = "PLACED"; // PLACED, COOKING, READY, SERVED, BILLED, CANCELLED
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = "UNPAID"; // UNPAID, PAID, CHARGED_TO_ROOM
    public string? PaymentMethod { get; set; }
    public long? ServerUserId { get; set; }
    public User? ServerUser { get; set; }
    public string? SpecialInstructions { get; set; }
    public string Source { get; set; } = "STAFF"; // STAFF, GUEST_QR
    public string? GuestDisplayName { get; set; }
    public string? DeliveryContactPhone { get; set; }
    public bool IsStayVerified { get; set; } = false;
    public int? EstimatedDeliveryMinutes { get; set; }
    public string? DeliveryStaffName { get; set; }
    public long? GuestOrderingSessionId { get; set; }
    public GuestOrderingSession? GuestOrderingSession { get; set; }
    public DateTimeOffset PlacedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? PreparedAt { get; set; }
    public DateTimeOffset? ServedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset? SettledAt { get; set; }
    public ICollection<RestaurantOrderItem> Items { get; set; } = new List<RestaurantOrderItem>();
}

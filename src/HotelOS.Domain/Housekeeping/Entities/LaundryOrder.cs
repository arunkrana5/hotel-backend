namespace HotelOS.Domain.Housekeeping.Entities;

using HotelOS.Domain.Common;
using HotelOS.Domain.Pms.Entities;

public class LaundryOrder : BaseEntity
{
    public long PropertyId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderType { get; set; } = "GUEST";
    public long? RoomId { get; set; }
    public Room? Room { get; set; }
    public long? GuestId { get; set; }
    public Guest? Guest { get; set; }
    public long? FolioId { get; set; }
    public Folio? Folio { get; set; }
    public string Status { get; set; } = "RECEIVED";
    public int TotalPieces { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsExpress { get; set; }
    public string? SpecialInstructions { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public ICollection<LaundryOrderItem> Items { get; set; } = new List<LaundryOrderItem>();
}

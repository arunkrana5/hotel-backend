using HotelOS.Domain.Common;

namespace HotelOS.Domain.Pms.Entities;

public class Reservation : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string ConfirmationNumber { get; set; } = string.Empty; // RES-2026-XXXXX
    public long GuestId { get; set; }
    public Guest Guest { get; set; } = null!;

    public string RoomTypeCode { get; set; } = string.Empty; // DLX, EXEC, OCN, etc.
    public long? AssignedRoomId { get; set; }
    public Room? AssignedRoom { get; set; }

    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int AdultsCount { get; set; } = 1;
    public int ChildrenCount { get; set; } = 0;

    public string ReservationStatus { get; set; } = "CONF"; // CONF, PROV, CHKI, CHKO, CANC, NOSH
    public string BookingSource { get; set; } = "DIRECT"; // DIRECT, WALKIN, CORP, OTA
    public decimal RatePerNight { get; set; }
    public decimal TotalEstimatedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? SpecialRequests { get; set; }

    public DateTimeOffset? ActualCheckInAt { get; set; }
    public DateTimeOffset? ActualCheckOutAt { get; set; }
    public string? CheckedInBy { get; set; }
    public string? CheckedOutBy { get; set; }

    public ICollection<Folio> Folios { get; set; } = new List<Folio>();
}

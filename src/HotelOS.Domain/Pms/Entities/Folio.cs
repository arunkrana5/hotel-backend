using HotelOS.Domain.Common;

namespace HotelOS.Domain.Pms.Entities;

public class Folio : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public long ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;

    public long GuestId { get; set; }
    public Guest Guest { get; set; } = null!;

    public string FolioNumber { get; set; } = string.Empty; // FOL-2026-XXXXX
    public string Status { get; set; } = "Open"; // Open, Settled, Closed
    public decimal TotalCharges { get; set; } = 0m;
    public decimal TotalPayments { get; set; } = 0m;
    public decimal BalanceAmount { get; set; } = 0m;
    public DateTimeOffset? ClosedAt { get; set; }

    public ICollection<FolioItem> Items { get; set; } = new List<FolioItem>();
}

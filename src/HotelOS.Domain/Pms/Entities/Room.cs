using HotelOS.Domain.Common;

namespace HotelOS.Domain.Pms.Entities;

public class Room : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string RoomNumber { get; set; } = string.Empty;
    public int FloorNumber { get; set; } = 1;
    public string RoomTypeCode { get; set; } = string.Empty; // DLX, EXEC, OCN, PRES, STD, etc.
    public string RoomStatusCode { get; set; } = "CLN"; // CLN, DRT, OCC, INS, OOD, MNT, DND
    public int MaxOccupancy { get; set; } = 2;
    public decimal BaseRatePerNight { get; set; }
    public bool IsSmoking { get; set; } = false;
    public string? KeyCardUid { get; set; }
    public string? Notes { get; set; }
    public string? ExtraDataJson { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

using HotelOS.Domain.Common;

namespace HotelOS.Domain.Pms.Entities;

public class Guest : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? IdDocumentType { get; set; } // Passport, NationalId, DriverLicense
    public string? IdDocumentNumber { get; set; }
    public string? Nationality { get; set; }
    public string VipStatus { get; set; } = "Standard"; // Standard, Silver, Gold, Platinum
    public int TotalStays { get; set; } = 0;
    public decimal LifetimeSpend { get; set; } = 0m;
    public string? SpecialPreferences { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Folio> Folios { get; set; } = new List<Folio>();
}

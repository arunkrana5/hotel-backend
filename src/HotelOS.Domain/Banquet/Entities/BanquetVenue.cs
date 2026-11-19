namespace HotelOS.Domain.Banquet.Entities;

using HotelOS.Domain.Common;

public class BanquetVenue : BaseEntity
{
    public long PropertyId { get; set; }
    public string VenueCode { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DayRate { get; set; }
    public string Status { get; set; } = "AVAILABLE"; // AVAILABLE, MAINTENANCE, BLOCKED
    public string? AmenitiesJson { get; set; }
}

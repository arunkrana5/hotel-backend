namespace HotelOS.Domain.Banquet.Entities;

using HotelOS.Domain.Common;
using HotelOS.Domain.Pms.Entities;

public class BanquetBooking : BaseEntity
{
    public long PropertyId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public long VenueId { get; set; }
    public BanquetVenue? Venue { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = "WEDDING"; // WEDDING, CONFERENCE, GALA, CORPORATE_MEETING, BIRTHDAY
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public long? GuestId { get; set; }
    public Guest? Guest { get; set; }
    public DateTimeOffset StartDateTime { get; set; }
    public DateTimeOffset EndDateTime { get; set; }
    public int ExpectedGuests { get; set; }
    public string Status { get; set; } = "INQUIRY"; // INQUIRY, CONFIRMED, IN_PROGRESS, COMPLETED, CANCELLED
    public decimal TotalAmount { get; set; }
    public decimal AdvancePaid { get; set; }
    public decimal BalanceAmount { get; set; }
    public string? CateringPackage { get; set; }
    public string? SetupStyle { get; set; } // ROUND_TABLE, THEATER, CLASSROOM, U_SHAPE
    public string? SpecialRequests { get; set; }
}

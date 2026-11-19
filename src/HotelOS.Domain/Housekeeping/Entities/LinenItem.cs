namespace HotelOS.Domain.Housekeeping.Entities;

using HotelOS.Domain.Common;

public class LinenItem : BaseEntity
{
    public long PropertyId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int ParLevel { get; set; }
    public int CleanStock { get; set; }
    public int InUseStock { get; set; }
    public int InLaundryStock { get; set; }
    public int DiscardedStock { get; set; }
    public string? Unit { get; set; } = "PCS";
}

using HotelOS.Domain.Common;

namespace HotelOS.Domain.Masters.Entities;

public class MasterValue : BaseEntity
{
    public long MasterDefinitionId { get; set; }
    public long? PropertyId { get; set; }
    public long? ParentValueId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? NumericValue { get; set; }
    public string? ExtraDataJson { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsDefault { get; set; } = false;

    public MasterDefinition? MasterDefinition { get; set; }
    public MasterValue? ParentValue { get; set; }
    public ICollection<MasterValue> ChildValues { get; set; } = new List<MasterValue>();
}

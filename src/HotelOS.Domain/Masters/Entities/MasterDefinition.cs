using HotelOS.Domain.Common;

namespace HotelOS.Domain.Masters.Entities;

public class MasterDefinition : BaseEntity
{
    public long CategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long? ParentDefinitionId { get; set; }
    public bool IsPropertySpecific { get; set; } = false;
    public bool IsHierarchical { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    public MasterCategory? Category { get; set; }
    public MasterDefinition? ParentDefinition { get; set; }
    public ICollection<MasterDefinition> ChildDefinitions { get; set; } = new List<MasterDefinition>();
    public ICollection<MasterValue> Values { get; set; } = new List<MasterValue>();
}

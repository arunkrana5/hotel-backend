using HotelOS.Domain.Common;

namespace HotelOS.Domain.Masters.Entities;

public class MasterCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystem { get; set; } = false;

    public ICollection<MasterDefinition> Definitions { get; set; } = new List<MasterDefinition>();
}

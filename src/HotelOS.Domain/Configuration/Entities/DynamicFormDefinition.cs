using HotelOS.Domain.Common;

namespace HotelOS.Domain.Configuration.Entities;

public class DynamicFormDefinition : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsPublished { get; set; } = true;

    public ICollection<DynamicFormField> Fields { get; set; } = new List<DynamicFormField>();
}

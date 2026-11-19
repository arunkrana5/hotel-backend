using HotelOS.Domain.Common;

namespace HotelOS.Domain.Configuration.Entities;

public class DynamicFormField : BaseEntity
{
    public long FormDefinitionId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text"; // text, number, date, select, textarea, checkbox, radio
    public bool IsMandatory { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public string? MasterDefinitionCode { get; set; } // If FieldType == 'select', binds to MasterDefinition
    public string? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public string? ValidationRulesJson { get; set; }
    public string? VisibilityRuleJson { get; set; }
    public string? SectionName { get; set; } = "General";

    public DynamicFormDefinition? FormDefinition { get; set; }
}

namespace HotelOS.Contracts.Configuration;

public record NavigationMenuDto(
    long Id,
    string Code,
    string Title,
    string Path,
    string? Icon,
    int DisplayOrder,
    string Module,
    string? BadgeText,
    List<NavigationMenuDto> Children
);

public record DynamicFormFieldDto(
    long Id,
    string FieldKey,
    string FieldLabel,
    string FieldType,
    bool IsMandatory,
    int DisplayOrder,
    string? MasterDefinitionCode,
    string? DefaultValue,
    string? Placeholder,
    string? SectionName
);

public record DynamicFormDto(
    long Id,
    string Code,
    string Title,
    string Description,
    string Module,
    string EntityType,
    int Version,
    List<DynamicFormFieldDto> Fields
);

namespace HotelOS.Contracts.Masters;

public record DropdownOptionDto(
    long Id,
    string Code,
    string Name,
    string Description,
    decimal? NumericValue = null,
    long? ParentValueId = null,
    bool IsDefault = false
);

public record MasterDefinitionDto(
    long Id,
    string Code,
    string Name,
    string CategoryCode,
    string Description,
    bool IsPropertySpecific,
    bool IsHierarchical,
    int DisplayOrder
);

public record MasterValueDto(
    long Id,
    long MasterDefinitionId,
    string DefinitionCode,
    long? PropertyId,
    long? ParentValueId,
    string Code,
    string Name,
    string Description,
    decimal? NumericValue,
    string? ExtraDataJson,
    int DisplayOrder,
    bool IsActive,
    bool IsDefault
);

public record CreateMasterValueRequest(
    long MasterDefinitionId,
    string Code,
    string Name,
    string Description,
    long? PropertyId = null,
    long? ParentValueId = null,
    decimal? NumericValue = null,
    string? ExtraDataJson = null,
    int DisplayOrder = 0,
    bool IsDefault = false
);

public record UpdateMasterValueRequest(
    string Name,
    string Description,
    long? ParentValueId = null,
    decimal? NumericValue = null,
    string? ExtraDataJson = null,
    int DisplayOrder = 0,
    bool IsActive = true,
    bool IsDefault = false
);

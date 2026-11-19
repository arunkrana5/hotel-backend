using HotelOS.Contracts.Common;
using HotelOS.Contracts.Masters;

namespace HotelOS.Application.Common.Interfaces;

public interface IMasterService
{
    Task<PagedResult<DropdownOptionDto>> GetDropdownOptionsAsync(
        string masterCode,
        int page,
        int pageSize,
        string? search,
        long? propertyId = null,
        long? parentValueId = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<MasterValueDto>> GetMasterValuesPagedAsync(
        string masterCode,
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<MasterValueDto>> CreateMasterValueAsync(
        CreateMasterValueRequest request,
        string username,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<MasterValueDto>> UpdateMasterValueAsync(
        long id,
        UpdateMasterValueRequest request,
        string username,
        CancellationToken cancellationToken = default);

    Task<ApiResponse> DeleteMasterValueAsync(
        long id,
        string username,
        CancellationToken cancellationToken = default);

    Task<List<MasterDefinitionDto>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default);
}

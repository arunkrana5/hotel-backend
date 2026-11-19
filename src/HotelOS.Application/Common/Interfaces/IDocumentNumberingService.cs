using HotelOS.Contracts.Common;
using HotelOS.Contracts.Configuration;

namespace HotelOS.Application.Common.Interfaces;

public interface IDocumentNumberingService
{
    Task<NextDocumentNumberResponse> PreviewNextNumberAsync(long propertyId, string documentType, CancellationToken cancellationToken = default);
    Task<NextDocumentNumberResponse> GenerateNextNumberAsync(long propertyId, string documentType, string generatedBy, CancellationToken cancellationToken = default);
    Task<List<DocumentNumberConfigurationDto>> GetConfigurationsAsync(long propertyId, CancellationToken cancellationToken = default);
    Task<DocumentNumberConfigurationDto?> UpdateConfigurationAsync(long id, UpdateDocumentNumberConfigRequest request, string updatedBy, CancellationToken cancellationToken = default);
    Task<PagedResult<DocumentNumberAuditDto>> GetAuditsPagedAsync(long propertyId, string? documentType, int page, int pageSize, CancellationToken cancellationToken = default);
}

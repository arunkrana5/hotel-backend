using HotelOS.Contracts.Configuration;

namespace HotelOS.Application.Common.Interfaces;

public interface IFormService
{
    Task<List<DynamicFormDto>> GetAllFormsAsync(CancellationToken cancellationToken = default);

    Task<DynamicFormDto?> GetFormByCodeAsync(
        string formCode,
        CancellationToken cancellationToken = default);
}

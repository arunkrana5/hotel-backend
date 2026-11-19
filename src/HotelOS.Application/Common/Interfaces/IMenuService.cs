using HotelOS.Contracts.Configuration;

namespace HotelOS.Application.Common.Interfaces;

public interface IMenuService
{
    Task<List<NavigationMenuDto>> GetAuthorizedMenusAsync(
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default);
}

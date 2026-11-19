using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/menus")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly ICurrentUserService _currentUser;

    public MenuController(IMenuService menuService, ICurrentUserService currentUser)
    {
        _menuService = menuService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuthorizedMenus(CancellationToken cancellationToken)
    {
        var menus = await _menuService.GetAuthorizedMenusAsync(
            _currentUser.Roles,
            _currentUser.Permissions,
            cancellationToken);

        return Ok(ApiResponse<List<NavigationMenuDto>>.Ok(menus));
    }
}

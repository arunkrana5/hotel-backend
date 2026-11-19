using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/masters")]
[Authorize]
public class MasterController : ControllerBase
{
    private readonly IMasterService _masterService;
    private readonly ICurrentUserService _currentUser;

    public MasterController(IMasterService masterService, ICurrentUserService currentUser)
    {
        _masterService = masterService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Standardized Dropdown Options Endpoint.
    /// Strictly enforces a maximum initial and incremental batch size of 5 records.
    /// </summary>
    [HttpGet("{code}/options")]
    [AllowAnonymous] // Allow public / unauthenticated dropdown options if needed (e.g. for registration) or authenticated
    public async Task<IActionResult> GetOptions(
        [FromRoute] string code,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string? search = null,
        [FromQuery] long? propertyId = null,
        [FromQuery] long? parentValueId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _masterService.GetDropdownOptionsAsync(
            code,
            page,
            pageSize,
            search,
            propertyId,
            parentValueId,
            cancellationToken);

        return Ok(ApiResponse<PagedResult<DropdownOptionDto>>.Ok(result));
    }

    /// <summary>
    /// Universal Server-Side DataTable query for Master Values.
    /// Supports pagination (10, 20, 50, 100), searching, and filtering.
    /// </summary>
    [HttpGet("{code}/values")]
    public async Task<IActionResult> GetValues(
        [FromRoute] string code,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _masterService.GetMasterValuesPagedAsync(
            code,
            page,
            pageSize,
            search,
            cancellationToken);

        return Ok(ApiResponse<PagedResult<MasterValueDto>>.Ok(result));
    }

    [HttpGet("definitions")]
    public async Task<IActionResult> GetDefinitions(CancellationToken cancellationToken = default)
    {
        var result = await _masterService.GetAllDefinitionsAsync(cancellationToken);
        return Ok(ApiResponse<List<MasterDefinitionDto>>.Ok(result));
    }

    [HttpPost("values")]
    public async Task<IActionResult> CreateValue(
        [FromBody] CreateMasterValueRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _masterService.CreateMasterValueAsync(
            request,
            _currentUser.Username ?? "SYSTEM",
            cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPut("values/{id}")]
    public async Task<IActionResult> UpdateValue(
        [FromRoute] long id,
        [FromBody] UpdateMasterValueRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _masterService.UpdateMasterValueAsync(
            id,
            request,
            _currentUser.Username ?? "SYSTEM",
            cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpDelete("values/{id}")]
    public async Task<IActionResult> DeleteValue(
        [FromRoute] long id,
        CancellationToken cancellationToken = default)
    {
        var result = await _masterService.DeleteMasterValueAsync(
            id,
            _currentUser.Username ?? "SYSTEM",
            cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
}

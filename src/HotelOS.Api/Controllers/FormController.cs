using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/forms")]
[Authorize]
public class FormController : ControllerBase
{
    private readonly IFormService _formService;

    public FormController(IFormService formService)
    {
        _formService = formService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllForms(CancellationToken cancellationToken)
    {
        var result = await _formService.GetAllFormsAsync(cancellationToken);
        return Ok(ApiResponse<List<DynamicFormDto>>.Ok(result));
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetFormByCode([FromRoute] string code, CancellationToken cancellationToken)
    {
        var result = await _formService.GetFormByCodeAsync(code, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Form '{code}' not found or not published.", 404));

        return Ok(ApiResponse<DynamicFormDto>.Ok(result));
    }
}

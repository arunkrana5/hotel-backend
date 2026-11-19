using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/documents")]
[Authorize]
public class DocumentNumberingController : ControllerBase
{
    private readonly IDocumentNumberingService _numberingService;
    private readonly ICurrentUserService _currentUser;

    public DocumentNumberingController(
        IDocumentNumberingService numberingService,
        ICurrentUserService currentUser)
    {
        _numberingService = numberingService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Preview the next document number without advancing the sequence.
    /// </summary>
    [HttpGet("next-preview")]
    public async Task<IActionResult> GetNextPreview(
        [FromQuery] string documentType,
        [FromQuery] long? propertyId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentType))
        {
            return BadRequest(ApiResponse<string>.Fail("Document type is required."));
        }

        long propId = propertyId ?? _currentUser.PropertyId ?? 1;
        var result = await _numberingService.PreviewNextNumberAsync(propId, documentType.ToUpperInvariant(), cancellationToken);
        return Ok(ApiResponse<NextDocumentNumberResponse>.Ok(result));
    }

    /// <summary>
    /// Concurrency-safe generation and reservation of the next document number.
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateNextNumber(
        [FromQuery] string documentType,
        [FromQuery] long? propertyId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentType))
        {
            return BadRequest(ApiResponse<string>.Fail("Document type is required."));
        }

        long propId = propertyId ?? _currentUser.PropertyId ?? 1;
        string userName = _currentUser.Username ?? "SYSTEM";
        var result = await _numberingService.GenerateNextNumberAsync(propId, documentType.ToUpperInvariant(), userName, cancellationToken);
        return Ok(ApiResponse<NextDocumentNumberResponse>.Ok(result));
    }

    /// <summary>
    /// Get all document numbering configurations for a property.
    /// </summary>
    [HttpGet("configurations")]
    public async Task<IActionResult> GetConfigurations(
        [FromQuery] long? propertyId,
        CancellationToken cancellationToken)
    {
        long propId = propertyId ?? _currentUser.PropertyId ?? 1;
        var configs = await _numberingService.GetConfigurationsAsync(propId, cancellationToken);
        return Ok(ApiResponse<List<DocumentNumberConfigurationDto>>.Ok(configs));
    }

    /// <summary>
    /// Update an existing document numbering configuration.
    /// </summary>
    [HttpPut("configurations/{id:long}")]
    public async Task<IActionResult> UpdateConfiguration(
        [FromRoute] long id,
        [FromBody] UpdateDocumentNumberConfigRequest request,
        CancellationToken cancellationToken)
    {
        string userName = _currentUser.Username ?? "SYSTEM";
        var updated = await _numberingService.UpdateConfigurationAsync(id, request, userName, cancellationToken);
        if (updated == null)
        {
            return NotFound(ApiResponse<string>.Fail($"Document numbering configuration with ID {id} not found.", 404));
        }
        return Ok(ApiResponse<DocumentNumberConfigurationDto>.Ok(updated));
    }

    /// <summary>
    /// Get audit history for generated document numbers.
    /// </summary>
    [HttpGet("audits")]
    public async Task<IActionResult> GetAudits(
        [FromQuery] long? propertyId,
        [FromQuery] string? documentType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        long propId = propertyId ?? _currentUser.PropertyId ?? 1;
        var audits = await _numberingService.GetAuditsPagedAsync(propId, documentType, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<DocumentNumberAuditDto>>.Ok(audits));
    }
}

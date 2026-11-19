using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/finance")]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _financeService;
    private readonly ICurrentUserService _currentUser;

    public FinanceController(
        IFinanceService financeService,
        ICurrentUserService currentUser)
    {
        _financeService = financeService;
        _currentUser = currentUser;
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts(
        [FromQuery] string? accountType = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _financeService.GetAccountsAsync(propertyId, accountType, cancellationToken);
        return Ok(ApiResponse<List<AccountDto>>.Ok(result));
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount(
        [FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _financeService.CreateAccountAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<AccountDto>.Ok(result, "GL Account registered successfully."));
    }

    [HttpGet("journal-entries")]
    public async Task<IActionResult> GetJournalEntries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _financeService.GetJournalEntriesPagedAsync(propertyId, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<JournalEntryDto>>.Ok(result));
    }

    [HttpPost("journal-entries")]
    public async Task<IActionResult> CreateJournalEntry(
        [FromBody] CreateJournalEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        try
        {
            var result = await _financeService.CreateJournalEntryAsync(request, propertyId, user, cancellationToken);
            return Ok(ApiResponse<JournalEntryDto>.Ok(result, "Journal entry posted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<JournalEntryDto>.Fail(ex.Message));
        }
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? invoiceType = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        var result = await _financeService.GetInvoicesPagedAsync(propertyId, page, pageSize, invoiceType, status, cancellationToken);
        return Ok(ApiResponse<PagedResult<InvoiceDto>>.Ok(result));
    }

    [HttpPost("invoices")]
    public async Task<IActionResult> CreateInvoice(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        long propertyId = _currentUser.PropertyId ?? 1;
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _financeService.CreateInvoiceAsync(request, propertyId, user, cancellationToken);
        return Ok(ApiResponse<InvoiceDto>.Ok(result, "Invoice generated successfully."));
    }

    [HttpPost("invoices/{id}/payments")]
    public async Task<IActionResult> RecordPayment(
        [FromRoute] long id,
        [FromBody] RecordInvoicePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        string user = _currentUser.Username ?? "SYSTEM";
        var result = await _financeService.RecordPaymentAsync(id, request, user, cancellationToken);
        if (result == null) return NotFound(ApiResponse<InvoiceDto>.Fail("Invoice not found."));
        return Ok(ApiResponse<InvoiceDto>.Ok(result, "Payment recorded successfully."));
    }
}

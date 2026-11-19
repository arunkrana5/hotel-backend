using HotelOS.Contracts.Common;
using HotelOS.Contracts.Finance;

namespace HotelOS.Application.Common.Interfaces;

public interface IFinanceService
{
    // Chart of Accounts
    Task<List<AccountDto>> GetAccountsAsync(
        long propertyId = 1,
        string? accountType = null,
        CancellationToken cancellationToken = default);

    Task<AccountDto> CreateAccountAsync(
        CreateAccountRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    // General Ledger Journal Entries
    Task<PagedResult<JournalEntryDto>> GetJournalEntriesPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<JournalEntryDto> CreateJournalEntryAsync(
        CreateJournalEntryRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    // Invoices
    Task<PagedResult<InvoiceDto>> GetInvoicesPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? invoiceType = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<InvoiceDto> CreateInvoiceAsync(
        CreateInvoiceRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<InvoiceDto?> RecordPaymentAsync(
        long invoiceId,
        RecordInvoicePaymentRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

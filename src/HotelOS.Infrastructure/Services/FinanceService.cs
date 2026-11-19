using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Finance;
using HotelOS.Domain.Finance.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class FinanceService : IFinanceService
{
    private readonly HotelOSDbContext _context;

    public FinanceService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccountDto>> GetAccountsAsync(
        long propertyId = 1,
        string? accountType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Accounts
            .AsNoTracking()
            .Where(a => a.PropertyId == propertyId && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(accountType))
            query = query.Where(a => a.AccountType == accountType.Trim().ToUpperInvariant());

        return await query
            .OrderBy(a => a.AccountCode)
            .Select(a => new AccountDto(
                a.Id,
                a.AccountCode,
                a.AccountName,
                a.AccountType,
                a.Balance,
                a.Description
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<AccountDto> CreateAccountAsync(
        CreateAccountRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var account = new Account
        {
            PropertyId = propertyId,
            AccountCode = request.AccountCode.Trim().ToUpperInvariant(),
            AccountName = request.AccountName,
            AccountType = request.AccountType.Trim().ToUpperInvariant(),
            Balance = request.InitialBalance,
            Description = request.Description,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        return new AccountDto(
            account.Id,
            account.AccountCode,
            account.AccountName,
            account.AccountType,
            account.Balance,
            account.Description
        );
    }

    public async Task<PagedResult<JournalEntryDto>> GetJournalEntriesPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .AsNoTracking()
            .Where(j => j.PropertyId == propertyId && !j.IsDeleted);

        var total = await query.CountAsync(cancellationToken);
        var entries = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = entries.Select(j => new JournalEntryDto(
            j.Id,
            j.EntryNumber,
            j.EntryDate.ToString("yyyy-MM-dd"),
            j.Description,
            j.ReferenceNumber,
            j.Status,
            j.TotalDebit,
            j.TotalCredit,
            j.CreatedAt,
            j.Lines.Select(l => new JournalLineDto(
                l.Id,
                l.AccountId,
                l.Account != null ? l.Account.AccountCode : string.Empty,
                l.Account != null ? l.Account.AccountName : string.Empty,
                l.DebitAmount,
                l.CreditAmount,
                l.Description
            )).ToList()
        )).ToList();

        return new PagedResult<JournalEntryDto> { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<JournalEntryDto> CreateJournalEntryAsync(
        CreateJournalEntryRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        decimal totalDebit = request.Lines.Sum(l => l.DebitAmount);
        decimal totalCredit = request.Lines.Sum(l => l.CreditAmount);

        if (totalDebit != totalCredit)
        {
            throw new InvalidOperationException($"Double-entry accounting violation: Total Debits (${totalDebit:F2}) must exactly equal Total Credits (${totalCredit:F2}).");
        }

        var countToday = await _context.JournalEntries
            .CountAsync(j => j.PropertyId == propertyId && j.CreatedAt.Date == DateTimeOffset.UtcNow.Date, cancellationToken);

        var entryNumber = $"JV-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";

        var lines = new List<JournalLine>();
        foreach (var lineReq in request.Lines)
        {
            var acc = await _context.Accounts.FindAsync([lineReq.AccountId], cancellationToken)
                ?? throw new KeyNotFoundException($"Account #{lineReq.AccountId} not found.");

            // Update account balance based on normal balances
            if (acc.AccountType == "ASSET" || acc.AccountType == "EXPENSE")
            {
                acc.Balance += (lineReq.DebitAmount - lineReq.CreditAmount);
            }
            else
            {
                acc.Balance += (lineReq.CreditAmount - lineReq.DebitAmount);
            }
            acc.ModifiedAt = DateTimeOffset.UtcNow;
            acc.ModifiedBy = createdBy;

            lines.Add(new JournalLine
            {
                AccountId = acc.Id,
                DebitAmount = lineReq.DebitAmount,
                CreditAmount = lineReq.CreditAmount,
                Description = lineReq.Description,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = createdBy
            });
        }

        var entry = new JournalEntry
        {
            PropertyId = propertyId,
            EntryNumber = entryNumber,
            EntryDate = DateOnly.Parse(request.EntryDate),
            Description = request.Description,
            ReferenceNumber = request.ReferenceNumber,
            Status = "POSTED",
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            Lines = lines,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return new JournalEntryDto(
            entry.Id,
            entry.EntryNumber,
            entry.EntryDate.ToString("yyyy-MM-dd"),
            entry.Description,
            entry.ReferenceNumber,
            entry.Status,
            entry.TotalDebit,
            entry.TotalCredit,
            entry.CreatedAt,
            lines.Select(l => new JournalLineDto(
                l.Id,
                l.AccountId,
                "",
                "",
                l.DebitAmount,
                l.CreditAmount,
                l.Description
            )).ToList()
        );
    }

    public async Task<PagedResult<InvoiceDto>> GetInvoicesPagedAsync(
        long propertyId = 1,
        int page = 1,
        int pageSize = 10,
        string? invoiceType = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Where(i => i.PropertyId == propertyId && !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(invoiceType))
            query = query.Where(i => i.InvoiceType == invoiceType.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status == status.Trim().ToUpperInvariant());

        var total = await query.CountAsync(cancellationToken);
        var invoices = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InvoiceDto(
                i.Id,
                i.InvoiceNumber,
                i.InvoiceType,
                i.ReferenceId,
                i.ClientOrVendorName,
                i.SubTotal,
                i.TaxAmount,
                i.TotalAmount,
                i.PaidAmount,
                i.BalanceAmount,
                i.IssueDate.ToString("yyyy-MM-dd"),
                i.DueDate.ToString("yyyy-MM-dd"),
                i.Status,
                i.Notes
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<InvoiceDto> { Items = invoices, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(
        CreateInvoiceRequest request,
        long propertyId,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var countToday = await _context.Invoices
            .CountAsync(i => i.PropertyId == propertyId && i.CreatedAt.Date == DateTimeOffset.UtcNow.Date, cancellationToken);

        var invNumber = $"INV-{DateTimeOffset.UtcNow:yyyyMMdd}-{countToday + 1:D4}";
        var total = request.SubTotal + request.TaxAmount;

        var inv = new Invoice
        {
            PropertyId = propertyId,
            InvoiceNumber = invNumber,
            InvoiceType = request.InvoiceType.Trim().ToUpperInvariant(),
            ReferenceId = request.ReferenceId,
            ClientOrVendorName = request.ClientOrVendorName,
            SubTotal = request.SubTotal,
            TaxAmount = request.TaxAmount,
            TotalAmount = total,
            PaidAmount = 0,
            BalanceAmount = total,
            IssueDate = DateOnly.Parse(request.IssueDate),
            DueDate = DateOnly.Parse(request.DueDate),
            Status = "ISSUED",
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        _context.Invoices.Add(inv);
        await _context.SaveChangesAsync(cancellationToken);

        return new InvoiceDto(
            inv.Id,
            inv.InvoiceNumber,
            inv.InvoiceType,
            inv.ReferenceId,
            inv.ClientOrVendorName,
            inv.SubTotal,
            inv.TaxAmount,
            inv.TotalAmount,
            inv.PaidAmount,
            inv.BalanceAmount,
            inv.IssueDate.ToString("yyyy-MM-dd"),
            inv.DueDate.ToString("yyyy-MM-dd"),
            inv.Status,
            inv.Notes
        );
    }

    public async Task<InvoiceDto?> RecordPaymentAsync(
        long invoiceId,
        RecordInvoicePaymentRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var inv = await _context.Invoices.FindAsync([invoiceId], cancellationToken);
        if (inv == null) return null;

        inv.PaidAmount += request.PaymentAmount;
        inv.BalanceAmount = Math.Max(0, inv.TotalAmount - inv.PaidAmount);

        if (inv.BalanceAmount == 0)
        {
            inv.Status = "PAID";
        }
        else
        {
            inv.Status = "PARTIALLY_PAID";
        }

        inv.ModifiedAt = DateTimeOffset.UtcNow;
        inv.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new InvoiceDto(
            inv.Id,
            inv.InvoiceNumber,
            inv.InvoiceType,
            inv.ReferenceId,
            inv.ClientOrVendorName,
            inv.SubTotal,
            inv.TaxAmount,
            inv.TotalAmount,
            inv.PaidAmount,
            inv.BalanceAmount,
            inv.IssueDate.ToString("yyyy-MM-dd"),
            inv.DueDate.ToString("yyyy-MM-dd"),
            inv.Status,
            inv.Notes
        );
    }
}

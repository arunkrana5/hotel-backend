namespace HotelOS.Domain.Finance.Entities;

using HotelOS.Domain.Common;

public class Account : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = "ASSET"; // ASSET, LIABILITY, EQUITY, REVENUE, EXPENSE
    public decimal Balance { get; set; }
    public string? Description { get; set; }
}

public class JournalEntry : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string EntryNumber { get; set; } = string.Empty;
    public DateOnly EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string Status { get; set; } = "POSTED"; // DRAFT, POSTED, VOID
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();
}

public class JournalLine : BaseEntity
{
    public long JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public long AccountId { get; set; }
    public Account? Account { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? Description { get; set; }
}

public class Invoice : BaseEntity
{
    public long PropertyId { get; set; } = 1;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceType { get; set; } = "GUEST_BILL"; // GUEST_BILL, VENDOR_INVOICE, BANQUET_INVOICE
    public string? ReferenceId { get; set; } // Folio #, PO #, Banquet #
    public string ClientOrVendorName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = "ISSUED"; // ISSUED, PARTIALLY_PAID, PAID, OVERDUE, VOID
    public string? Notes { get; set; }
}

namespace HotelOS.Contracts.Finance;

public record AccountDto(
    long Id,
    string AccountCode,
    string AccountName,
    string AccountType,
    decimal Balance,
    string? Description
);

public record CreateAccountRequest(
    string AccountCode,
    string AccountName,
    string AccountType,
    decimal InitialBalance,
    string? Description
);

public record JournalLineDto(
    long Id,
    long AccountId,
    string AccountCode,
    string AccountName,
    decimal DebitAmount,
    decimal CreditAmount,
    string? Description
);

public record CreateJournalLineRequest(
    long AccountId,
    decimal DebitAmount,
    decimal CreditAmount,
    string? Description
);

public record JournalEntryDto(
    long Id,
    string EntryNumber,
    string EntryDate,
    string Description,
    string? ReferenceNumber,
    string Status,
    decimal TotalDebit,
    decimal TotalCredit,
    DateTimeOffset CreatedAt,
    List<JournalLineDto> Lines
);

public record CreateJournalEntryRequest(
    string EntryDate,
    string Description,
    string? ReferenceNumber,
    List<CreateJournalLineRequest> Lines
);

public record InvoiceDto(
    long Id,
    string InvoiceNumber,
    string InvoiceType,
    string? ReferenceId,
    string ClientOrVendorName,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceAmount,
    string IssueDate,
    string DueDate,
    string Status,
    string? Notes
);

public record CreateInvoiceRequest(
    string InvoiceType,
    string? ReferenceId,
    string ClientOrVendorName,
    decimal SubTotal,
    decimal TaxAmount,
    string IssueDate,
    string DueDate,
    string? Notes
);

public record RecordInvoicePaymentRequest(
    decimal PaymentAmount,
    string? PaymentMethod
);

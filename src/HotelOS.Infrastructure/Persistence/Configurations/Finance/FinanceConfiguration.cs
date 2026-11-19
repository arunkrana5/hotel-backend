using HotelOS.Domain.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Finance;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts", "finance");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AccountCode).HasMaxLength(32).IsRequired();
        builder.Property(a => a.AccountName).HasMaxLength(150).IsRequired();
        builder.Property(a => a.AccountType).HasMaxLength(32).IsRequired();
        builder.Property(a => a.Balance).HasPrecision(18, 2);

        builder.HasIndex(a => new { a.PropertyId, a.AccountCode }).IsUnique();
        builder.HasIndex(a => a.AccountType);
    }
}

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("journal_entries", "finance");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.EntryNumber).HasMaxLength(64).IsRequired();
        builder.Property(j => j.Description).HasMaxLength(500).IsRequired();
        builder.Property(j => j.Status).HasMaxLength(32).HasDefaultValue("POSTED");
        builder.Property(j => j.TotalDebit).HasPrecision(18, 2);
        builder.Property(j => j.TotalCredit).HasPrecision(18, 2);

        builder.HasIndex(j => new { j.PropertyId, j.EntryNumber }).IsUnique();

        builder.HasMany(j => j.Lines)
            .WithOne(l => l.JournalEntry)
            .HasForeignKey(l => l.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class JournalLineConfiguration : IEntityTypeConfiguration<JournalLine>
{
    public void Configure(EntityTypeBuilder<JournalLine> builder)
    {
        builder.ToTable("journal_lines", "finance");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.DebitAmount).HasPrecision(18, 2);
        builder.Property(l => l.CreditAmount).HasPrecision(18, 2);

        builder.HasOne(l => l.Account)
            .WithMany()
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices", "finance");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.InvoiceNumber).HasMaxLength(64).IsRequired();
        builder.Property(i => i.InvoiceType).HasMaxLength(32).IsRequired();
        builder.Property(i => i.ClientOrVendorName).HasMaxLength(150).IsRequired();
        builder.Property(i => i.SubTotal).HasPrecision(18, 2);
        builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
        builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
        builder.Property(i => i.PaidAmount).HasPrecision(18, 2);
        builder.Property(i => i.BalanceAmount).HasPrecision(18, 2);
        builder.Property(i => i.Status).HasMaxLength(32).HasDefaultValue("ISSUED");

        builder.HasIndex(i => new { i.PropertyId, i.InvoiceNumber }).IsUnique();
        builder.HasIndex(i => i.Status);
    }
}

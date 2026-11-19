using HotelOS.Domain.Pms.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Pms;

public class FolioItemConfiguration : IEntityTypeConfiguration<FolioItem>
{
    public void Configure(EntityTypeBuilder<FolioItem> builder)
    {
        builder.ToTable("folio_items", "pms");
        builder.HasKey(fi => fi.Id);

        builder.Property(fi => fi.ItemType).HasMaxLength(32).IsRequired();
        builder.Property(fi => fi.DepartmentCode).HasMaxLength(32).IsRequired();
        builder.Property(fi => fi.Description).HasMaxLength(256).IsRequired();
        builder.Property(fi => fi.UnitPrice).HasPrecision(18, 2);
        builder.Property(fi => fi.Amount).HasPrecision(18, 2);
        builder.Property(fi => fi.PaymentModeCode).HasMaxLength(32);
        builder.Property(fi => fi.TransactionReference).HasMaxLength(128);
        builder.Property(fi => fi.PostedBy).HasMaxLength(100).IsRequired();

        builder.HasOne(fi => fi.Folio)
            .WithMany(f => f.Items)
            .HasForeignKey(fi => fi.FolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

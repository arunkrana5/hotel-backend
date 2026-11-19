using HotelOS.Domain.Pms.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Pms;

public class FolioConfiguration : IEntityTypeConfiguration<Folio>
{
    public void Configure(EntityTypeBuilder<Folio> builder)
    {
        builder.ToTable("folios", "pms");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FolioNumber).HasMaxLength(64).IsRequired();
        builder.Property(f => f.Status).HasMaxLength(32).IsRequired().HasDefaultValue("Open");
        builder.Property(f => f.TotalCharges).HasPrecision(18, 2);
        builder.Property(f => f.TotalPayments).HasPrecision(18, 2);
        builder.Property(f => f.BalanceAmount).HasPrecision(18, 2);

        builder.HasIndex(f => new { f.PropertyId, f.FolioNumber }).IsUnique();
        builder.HasIndex(f => f.Status);

        builder.HasOne(f => f.Reservation)
            .WithMany(res => res.Folios)
            .HasForeignKey(f => f.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Guest)
            .WithMany(g => g.Folios)
            .HasForeignKey(f => f.GuestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

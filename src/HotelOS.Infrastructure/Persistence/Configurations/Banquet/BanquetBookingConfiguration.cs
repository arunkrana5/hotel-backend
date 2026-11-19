using HotelOS.Domain.Banquet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Banquet;

public class BanquetBookingConfiguration : IEntityTypeConfiguration<BanquetBooking>
{
    public void Configure(EntityTypeBuilder<BanquetBooking> builder)
    {
        builder.ToTable("banquet_bookings", "banquet");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BookingNumber).HasMaxLength(64).IsRequired();
        builder.Property(b => b.EventName).HasMaxLength(200).IsRequired();
        builder.Property(b => b.EventType).HasMaxLength(64).IsRequired().HasDefaultValue("WEDDING");
        builder.Property(b => b.ClientName).HasMaxLength(150).IsRequired();
        builder.Property(b => b.ClientPhone).HasMaxLength(32).IsRequired();
        builder.Property(b => b.ClientEmail).HasMaxLength(150);
        builder.Property(b => b.Status).HasMaxLength(32).IsRequired().HasDefaultValue("INQUIRY");
        builder.Property(b => b.SetupStyle).HasMaxLength(64);
        builder.Property(b => b.CateringPackage).HasMaxLength(100);
        builder.Property(b => b.SpecialRequests).HasMaxLength(500);

        builder.Property(b => b.TotalAmount).HasPrecision(18, 2);
        builder.Property(b => b.AdvancePaid).HasPrecision(18, 2);
        builder.Property(b => b.BalanceAmount).HasPrecision(18, 2);

        builder.HasIndex(b => new { b.PropertyId, b.BookingNumber }).IsUnique();
        builder.HasIndex(b => b.VenueId);
        builder.HasIndex(b => b.Status);

        builder.HasOne(b => b.Venue)
            .WithMany()
            .HasForeignKey(b => b.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Guest)
            .WithMany()
            .HasForeignKey(b => b.GuestId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

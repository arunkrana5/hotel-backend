using HotelOS.Domain.Pms.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Pms;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations", "pms");
        builder.HasKey(res => res.Id);

        builder.Property(res => res.ConfirmationNumber).HasMaxLength(64).IsRequired();
        builder.Property(res => res.RoomTypeCode).HasMaxLength(32).IsRequired();
        builder.Property(res => res.ReservationStatus).HasMaxLength(32).IsRequired().HasDefaultValue("CONF");
        builder.Property(res => res.BookingSource).HasMaxLength(32).IsRequired().HasDefaultValue("DIRECT");
        builder.Property(res => res.RatePerNight).HasPrecision(18, 2);
        builder.Property(res => res.TotalEstimatedAmount).HasPrecision(18, 2);
        builder.Property(res => res.PaidAmount).HasPrecision(18, 2);
        builder.Property(res => res.SpecialRequests).HasMaxLength(1000);
        builder.Property(res => res.CheckedInBy).HasMaxLength(100);
        builder.Property(res => res.CheckedOutBy).HasMaxLength(100);

        builder.HasIndex(res => new { res.PropertyId, res.ConfirmationNumber }).IsUnique();
        builder.HasIndex(res => res.ReservationStatus);
        builder.HasIndex(res => new { res.CheckInDate, res.CheckOutDate });

        builder.HasOne(res => res.Guest)
            .WithMany(g => g.Reservations)
            .HasForeignKey(res => res.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(res => res.AssignedRoom)
            .WithMany(r => r.Reservations)
            .HasForeignKey(res => res.AssignedRoomId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

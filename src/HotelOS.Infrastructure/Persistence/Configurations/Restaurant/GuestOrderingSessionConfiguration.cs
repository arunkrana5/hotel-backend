using HotelOS.Domain.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Restaurant;

public class GuestOrderingSessionConfiguration : IEntityTypeConfiguration<GuestOrderingSession>
{
    public void Configure(EntityTypeBuilder<GuestOrderingSession> builder)
    {
        builder.ToTable("guest_ordering_sessions", "restaurant");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SessionToken).HasMaxLength(128).IsRequired();
        builder.Property(s => s.GuestName).HasMaxLength(150);
        builder.Property(s => s.GuestPhone).HasMaxLength(32);
        builder.Property(s => s.VerificationMode).HasMaxLength(32).IsRequired().HasDefaultValue("AUTO");

        builder.HasIndex(s => s.SessionToken).IsUnique();
        builder.HasIndex(s => new { s.PropertyId, s.RoomId });

        builder.HasOne(s => s.Room)
            .WithMany()
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.RoomQrCode)
            .WithMany()
            .HasForeignKey(s => s.RoomQrCodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Reservation)
            .WithMany()
            .HasForeignKey(s => s.ReservationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.Guest)
            .WithMany()
            .HasForeignKey(s => s.GuestId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

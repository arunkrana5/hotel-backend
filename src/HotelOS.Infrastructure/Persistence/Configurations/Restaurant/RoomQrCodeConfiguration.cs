using HotelOS.Domain.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Restaurant;

public class RoomQrCodeConfiguration : IEntityTypeConfiguration<RoomQrCode>
{
    public void Configure(EntityTypeBuilder<RoomQrCode> builder)
    {
        builder.ToTable("room_qr_codes", "restaurant");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.QrToken).HasMaxLength(128).IsRequired();
        builder.Property(q => q.PinCode).HasMaxLength(16);

        builder.HasIndex(q => new { q.PropertyId, q.RoomId });
        builder.HasIndex(q => q.QrToken).IsUnique();

        builder.HasOne(q => q.Room)
            .WithMany()
            .HasForeignKey(q => q.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

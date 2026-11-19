using HotelOS.Domain.Pms.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Pms;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms", "pms");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomNumber).HasMaxLength(32).IsRequired();
        builder.Property(r => r.RoomTypeCode).HasMaxLength(32).IsRequired();
        builder.Property(r => r.RoomStatusCode).HasMaxLength(32).IsRequired().HasDefaultValue("CLN");
        builder.Property(r => r.BaseRatePerNight).HasPrecision(18, 2);
        builder.Property(r => r.KeyCardUid).HasMaxLength(64);
        builder.Property(r => r.Notes).HasMaxLength(500);

        builder.HasIndex(r => new { r.PropertyId, r.RoomNumber }).IsUnique();
        builder.HasIndex(r => r.RoomTypeCode);
        builder.HasIndex(r => r.RoomStatusCode);
    }
}

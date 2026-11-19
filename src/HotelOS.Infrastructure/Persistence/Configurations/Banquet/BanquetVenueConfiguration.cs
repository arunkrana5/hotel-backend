using HotelOS.Domain.Banquet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Banquet;

public class BanquetVenueConfiguration : IEntityTypeConfiguration<BanquetVenue>
{
    public void Configure(EntityTypeBuilder<BanquetVenue> builder)
    {
        builder.ToTable("banquet_venues", "banquet");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.VenueCode).HasMaxLength(32).IsRequired();
        builder.Property(v => v.VenueName).HasMaxLength(150).IsRequired();
        builder.Property(v => v.Location).HasMaxLength(150).IsRequired();
        builder.Property(v => v.HourlyRate).HasPrecision(18, 2);
        builder.Property(v => v.DayRate).HasPrecision(18, 2);
        builder.Property(v => v.Status).HasMaxLength(32).IsRequired().HasDefaultValue("AVAILABLE");
        builder.Property(v => v.AmenitiesJson).HasColumnType("jsonb");

        builder.HasIndex(v => new { v.PropertyId, v.VenueCode }).IsUnique();
        builder.HasIndex(v => v.Status);
    }
}

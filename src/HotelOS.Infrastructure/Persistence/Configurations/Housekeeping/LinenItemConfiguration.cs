using HotelOS.Domain.Housekeeping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Housekeeping;

public class LinenItemConfiguration : IEntityTypeConfiguration<LinenItem>
{
    public void Configure(EntityTypeBuilder<LinenItem> builder)
    {
        builder.ToTable("linen_items", "housekeeping");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ItemCode).HasMaxLength(64).IsRequired();
        builder.Property(l => l.ItemName).HasMaxLength(150).IsRequired();
        builder.Property(l => l.Category).HasMaxLength(64).IsRequired();
        builder.Property(l => l.Unit).HasMaxLength(32).HasDefaultValue("PCS");

        builder.HasIndex(l => new { l.PropertyId, l.ItemCode }).IsUnique();
        builder.HasIndex(l => l.Category);
    }
}

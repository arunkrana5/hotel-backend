using HotelOS.Domain.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Restaurant;

public class DiningTableConfiguration : IEntityTypeConfiguration<DiningTable>
{
    public void Configure(EntityTypeBuilder<DiningTable> builder)
    {
        builder.ToTable("dining_tables", "restaurant");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TableNumber).HasMaxLength(32).IsRequired();
        builder.Property(t => t.Section).HasMaxLength(64).IsRequired().HasDefaultValue("MAIN_DINING");
        builder.Property(t => t.Status).HasMaxLength(32).IsRequired().HasDefaultValue("AVAILABLE");

        builder.HasIndex(t => new { t.PropertyId, t.TableNumber }).IsUnique();
        builder.HasIndex(t => t.Section);
        builder.HasIndex(t => t.Status);
    }
}

using HotelOS.Domain.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Restaurant;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu_items", "restaurant");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ItemCode).HasMaxLength(64).IsRequired();
        builder.Property(m => m.ItemName).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Category).HasMaxLength(64).IsRequired().HasDefaultValue("MAIN_COURSE");
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.Price).HasPrecision(18, 2);
        builder.Property(m => m.DietaryType).HasMaxLength(32).IsRequired().HasDefaultValue("VEG");
        builder.Property(m => m.ImageUrl).HasMaxLength(500);

        builder.HasIndex(m => new { m.PropertyId, m.ItemCode }).IsUnique();
        builder.HasIndex(m => m.Category);
        builder.HasIndex(m => m.IsAvailable);
    }
}

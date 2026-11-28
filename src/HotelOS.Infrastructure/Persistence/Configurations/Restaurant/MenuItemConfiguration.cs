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
        builder.Property(m => m.TaxPercent).HasPrecision(5, 2).HasDefaultValue(5.0m);
        builder.Property(m => m.DietaryType).HasMaxLength(32).IsRequired().HasDefaultValue("VEG");
        builder.Property(m => m.Status).HasMaxLength(32).IsRequired().HasDefaultValue("PUBLISHED");
        builder.Property(m => m.ImageUrl).HasMaxLength(500);
        builder.Property(m => m.Allergens).HasMaxLength(250);
        builder.Property(m => m.VariantsJson).HasColumnType("text");
        builder.Property(m => m.AddOnsJson).HasColumnType("text");
        builder.Property(m => m.ApplicableOutlets).HasMaxLength(250).IsRequired().HasDefaultValue("ROOM_SERVICE,MAIN_RESTAURANT");

        builder.HasIndex(m => new { m.PropertyId, m.ItemCode }).IsUnique();
        builder.HasIndex(m => m.Category);
        builder.HasIndex(m => m.IsAvailable);
        builder.HasIndex(m => m.IsGuestVisible);
        builder.HasIndex(m => m.IsFeatured);

        builder.HasOne(m => m.Menu)
            .WithMany(menu => menu.Items)
            .HasForeignKey(m => m.MenuId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.MenuCategory)
            .WithMany(cat => cat.Items)
            .HasForeignKey(m => m.MenuCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

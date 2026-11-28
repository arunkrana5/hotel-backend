using HotelOS.Domain.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Restaurant;

public class MenuCategoryConfiguration : IEntityTypeConfiguration<MenuCategory>
{
    public void Configure(EntityTypeBuilder<MenuCategory> builder)
    {
        builder.ToTable("menu_categories", "restaurant");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CategoryCode).HasMaxLength(64).IsRequired();
        builder.Property(c => c.CategoryName).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.ApplicableOutlets).HasMaxLength(250).IsRequired().HasDefaultValue("ROOM_SERVICE,MAIN_RESTAURANT");

        builder.HasIndex(c => new { c.PropertyId, c.CategoryCode }).IsUnique();
        builder.HasIndex(c => c.DisplayOrder);
        builder.HasIndex(c => c.IsGuestVisible);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.MenuCategory)
            .HasForeignKey(i => i.MenuCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

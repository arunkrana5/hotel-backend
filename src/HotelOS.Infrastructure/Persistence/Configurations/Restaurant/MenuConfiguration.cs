using HotelOS.Domain.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Restaurant;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("menus", "restaurant");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MenuCode).HasMaxLength(64).IsRequired();
        builder.Property(m => m.MenuName).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.ImageUrl).HasMaxLength(500);
        builder.Property(m => m.MenuType).HasMaxLength(64).IsRequired().HasDefaultValue("ALL_DAY");
        builder.Property(m => m.Status).HasMaxLength(32).IsRequired().HasDefaultValue("PUBLISHED");
        builder.Property(m => m.AvailableDays).HasMaxLength(100).IsRequired().HasDefaultValue("MON,TUE,WED,THU,FRI,SAT,SUN");
        builder.Property(m => m.ApplicableOutlets).HasMaxLength(250).IsRequired().HasDefaultValue("ROOM_SERVICE,MAIN_RESTAURANT");

        builder.HasIndex(m => new { m.PropertyId, m.MenuCode }).IsUnique();
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.IsGuestVisible);

        builder.HasMany(m => m.Items)
            .WithOne(i => i.Menu)
            .HasForeignKey(i => i.MenuId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(m => m.Categories)
            .WithOne(c => c.Menu)
            .HasForeignKey(c => c.MenuId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

using HotelOS.Domain.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Restaurant;

public class RestaurantOrderConfiguration : IEntityTypeConfiguration<RestaurantOrder>
{
    public void Configure(EntityTypeBuilder<RestaurantOrder> builder)
    {
        builder.ToTable("restaurant_orders", "restaurant");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).HasMaxLength(64).IsRequired();
        builder.Property(o => o.OrderType).HasMaxLength(32).IsRequired().HasDefaultValue("DINE_IN");
        builder.Property(o => o.Status).HasMaxLength(32).IsRequired().HasDefaultValue("PLACED");
        builder.Property(o => o.PaymentStatus).HasMaxLength(32).IsRequired().HasDefaultValue("UNPAID");
        builder.Property(o => o.PaymentMethod).HasMaxLength(32);
        builder.Property(o => o.SpecialInstructions).HasMaxLength(500);
        builder.Property(o => o.Source).HasMaxLength(32).IsRequired().HasDefaultValue("STAFF");
        builder.Property(o => o.GuestDisplayName).HasMaxLength(150);
        builder.Property(o => o.DeliveryContactPhone).HasMaxLength(32);
        builder.Property(o => o.DeliveryStaffName).HasMaxLength(150);

        builder.Property(o => o.SubTotal).HasPrecision(18, 2);
        builder.Property(o => o.TaxAmount).HasPrecision(18, 2);
        builder.Property(o => o.DiscountAmount).HasPrecision(18, 2);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);

        builder.HasIndex(o => new { o.PropertyId, o.OrderNumber }).IsUnique();
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.OrderType);

        builder.HasOne(o => o.Table)
            .WithMany()
            .HasForeignKey(o => o.TableId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Room)
            .WithMany()
            .HasForeignKey(o => o.RoomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Guest)
            .WithMany()
            .HasForeignKey(o => o.GuestId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Folio)
            .WithMany()
            .HasForeignKey(o => o.FolioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.ServerUser)
            .WithMany()
            .HasForeignKey(o => o.ServerUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.RestaurantOrder)
            .HasForeignKey(i => i.RestaurantOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RestaurantOrderItemConfiguration : IEntityTypeConfiguration<RestaurantOrderItem>
{
    public void Configure(EntityTypeBuilder<RestaurantOrderItem> builder)
    {
        builder.ToTable("restaurant_order_items", "restaurant");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ItemName).HasMaxLength(150).IsRequired();
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.TotalPrice).HasPrecision(18, 2);
        builder.Property(i => i.Status).HasMaxLength(32).IsRequired().HasDefaultValue("QUEUED");
        builder.Property(i => i.Notes).HasMaxLength(300);

        builder.HasIndex(i => i.RestaurantOrderId);
        builder.HasIndex(i => i.Status);

        builder.HasOne(i => i.MenuItem)
            .WithMany()
            .HasForeignKey(i => i.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

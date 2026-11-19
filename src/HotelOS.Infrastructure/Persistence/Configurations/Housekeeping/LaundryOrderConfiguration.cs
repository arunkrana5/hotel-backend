using HotelOS.Domain.Housekeeping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Housekeeping;

public class LaundryOrderConfiguration : IEntityTypeConfiguration<LaundryOrder>
{
    public void Configure(EntityTypeBuilder<LaundryOrder> builder)
    {
        builder.ToTable("laundry_orders", "housekeeping");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).HasMaxLength(64).IsRequired();
        builder.Property(o => o.OrderType).HasMaxLength(32).IsRequired().HasDefaultValue("GUEST");
        builder.Property(o => o.Status).HasMaxLength(32).IsRequired().HasDefaultValue("RECEIVED");
        builder.Property(o => o.SubTotal).HasPrecision(18, 2);
        builder.Property(o => o.TaxAmount).HasPrecision(18, 2);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
        builder.Property(o => o.SpecialInstructions).HasMaxLength(500);

        builder.HasIndex(o => new { o.PropertyId, o.OrderNumber }).IsUnique();
        builder.HasIndex(o => o.Status);

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

        builder.HasMany(o => o.Items)
            .WithOne(i => i.LaundryOrder)
            .HasForeignKey(i => i.LaundryOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LaundryOrderItemConfiguration : IEntityTypeConfiguration<LaundryOrderItem>
{
    public void Configure(EntityTypeBuilder<LaundryOrderItem> builder)
    {
        builder.ToTable("laundry_order_items", "housekeeping");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ServiceType).HasMaxLength(64).IsRequired();
        builder.Property(i => i.GarmentName).HasMaxLength(150).IsRequired();
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.TotalPrice).HasPrecision(18, 2);
        builder.Property(i => i.Remarks).HasMaxLength(300);

        builder.HasIndex(i => i.LaundryOrderId);
    }
}

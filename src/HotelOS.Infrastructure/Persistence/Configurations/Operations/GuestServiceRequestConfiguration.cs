using HotelOS.Domain.Operations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Operations;

public class GuestServiceRequestConfiguration : IEntityTypeConfiguration<GuestServiceRequest>
{
    public void Configure(EntityTypeBuilder<GuestServiceRequest> builder)
    {
        builder.ToTable("guest_service_requests", "operations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RequestNumber).HasMaxLength(64).IsRequired();
        builder.Property(r => r.RequestType).HasMaxLength(64).IsRequired();
        builder.Property(r => r.Department).HasMaxLength(64).IsRequired().HasDefaultValue("HOUSEKEEPING");
        builder.Property(r => r.Priority).HasMaxLength(32).IsRequired().HasDefaultValue("MEDIUM");
        builder.Property(r => r.Status).HasMaxLength(32).IsRequired().HasDefaultValue("PENDING");
        builder.Property(r => r.Description).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.ResolutionNotes).HasMaxLength(1000);

        builder.HasIndex(r => new { r.PropertyId, r.RequestNumber }).IsUnique();
        builder.HasIndex(r => r.RoomId);
        builder.HasIndex(r => r.Department);
        builder.HasIndex(r => r.Status);

        builder.HasOne(r => r.Room)
            .WithMany()
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Guest)
            .WithMany()
            .HasForeignKey(r => r.GuestId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.AssignedToUser)
            .WithMany()
            .HasForeignKey(r => r.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

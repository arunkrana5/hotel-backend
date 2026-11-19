using HotelOS.Domain.Pms.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Pms;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.ToTable("guests", "pms");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(g => g.LastName).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Email).HasMaxLength(256).IsRequired();
        builder.Property(g => g.Phone).HasMaxLength(50).IsRequired();
        builder.Property(g => g.IdDocumentType).HasMaxLength(50);
        builder.Property(g => g.IdDocumentNumber).HasMaxLength(100);
        builder.Property(g => g.Nationality).HasMaxLength(100);
        builder.Property(g => g.VipStatus).HasMaxLength(50).HasDefaultValue("Standard");
        builder.Property(g => g.LifetimeSpend).HasPrecision(18, 2);
        builder.Property(g => g.SpecialPreferences).HasMaxLength(1000);
        builder.Property(g => g.Address).HasMaxLength(500);
        builder.Property(g => g.City).HasMaxLength(100);
        builder.Property(g => g.Country).HasMaxLength(100);

        builder.HasIndex(g => new { g.PropertyId, g.Email });
        builder.HasIndex(g => new { g.PropertyId, g.Phone });
    }
}

using HotelOS.Domain.Hr.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Hr;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees", "hr");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EmployeeCode).HasMaxLength(32).IsRequired();
        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DepartmentCode).HasMaxLength(32).IsRequired();
        builder.Property(e => e.Designation).HasMaxLength(100).IsRequired();
        builder.Property(e => e.EmploymentType).HasMaxLength(32).HasDefaultValue("FULL_TIME");
        builder.Property(e => e.BasicSalary).HasPrecision(18, 2);
        builder.Property(e => e.Status).HasMaxLength(32).HasDefaultValue("ACTIVE");

        builder.HasIndex(e => new { e.PropertyId, e.EmployeeCode }).IsUnique();
        builder.HasIndex(e => e.DepartmentCode);
        builder.HasIndex(e => e.Status);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("attendance_records", "hr");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Shift).HasMaxLength(32).HasDefaultValue("MORNING");
        builder.Property(a => a.Status).HasMaxLength(32).HasDefaultValue("PRESENT");
        builder.Property(a => a.OvertimeHours).HasPrecision(18, 2);

        builder.HasIndex(a => new { a.PropertyId, a.EmployeeId, a.Date }).IsUnique();
        builder.HasIndex(a => a.Date);

        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

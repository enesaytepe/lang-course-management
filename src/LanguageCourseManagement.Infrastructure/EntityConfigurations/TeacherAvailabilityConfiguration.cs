using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class TeacherAvailabilityConfiguration : IEntityTypeConfiguration<TeacherAvailability>
{
    public void Configure(EntityTypeBuilder<TeacherAvailability> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.DayOfWeek).HasConversion<int>();
        builder.Property(x => x.StartTime).HasColumnType("time(0)");
        builder.Property(x => x.EndTime).HasColumnType("time(0)");
        builder.ToTable(x => x.HasCheckConstraint("CK_TeacherAvailabilities_StartBeforeEnd", "[StartTime] < [EndTime]"));
        builder.HasIndex(x => new { x.TeacherId, x.DayOfWeek, x.StartTime, x.EndTime }).HasDatabaseName("UX_TeacherAvailabilities_Teacher_Day_Time").IsUnique();
        builder.HasOne(x => x.Teacher).WithMany(x => x.Availabilities).HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.Teacher.IsDeleted);
    }
}

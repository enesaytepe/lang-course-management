using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class CourseScheduleConfiguration : IEntityTypeConfiguration<CourseSchedule>
{
    public void Configure(EntityTypeBuilder<CourseSchedule> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.DayOfWeek).HasConversion<int>();
        builder.Property(x => x.StartTime).HasColumnType("time(0)");
        builder.Property(x => x.EndTime).HasColumnType("time(0)");
        builder.ToTable(x => x.HasCheckConstraint("CK_CourseSchedules_StartBeforeEnd", "[StartTime] < [EndTime]"));
        builder.HasIndex(x => new { x.CourseId, x.DayOfWeek, x.StartTime, x.EndTime }).HasDatabaseName("UX_CourseSchedules_Course_Day_Time").IsUnique();
        builder.HasOne(x => x.Course).WithMany(x => x.Schedules).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.Course.IsDeleted);
    }
}

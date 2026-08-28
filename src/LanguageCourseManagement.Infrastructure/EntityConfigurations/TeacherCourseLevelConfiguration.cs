using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class TeacherCourseLevelConfiguration : IEntityTypeConfiguration<TeacherCourseLevel>
{
    public void Configure(EntityTypeBuilder<TeacherCourseLevel> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.HasIndex(x => new { x.TeacherId, x.CourseLevelId }).HasDatabaseName("UX_TeacherCourseLevels_Teacher_CourseLevel").IsUnique();
        builder.HasOne(x => x.Teacher).WithMany(x => x.TeacherCourseLevels).HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CourseLevel).WithMany(x => x.TeacherCourseLevels).HasForeignKey(x => x.CourseLevelId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.Teacher.IsDeleted);
    }
}

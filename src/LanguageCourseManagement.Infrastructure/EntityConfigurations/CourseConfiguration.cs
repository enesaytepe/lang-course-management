using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.TuitionFee).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.ToTable(x =>
        {
            x.HasCheckConstraint("CK_Courses_Capacity_Positive", "[Capacity] > 0");
            x.HasCheckConstraint("CK_Courses_TuitionFee_NonNegative", "[TuitionFee] >= 0");
            x.HasCheckConstraint("CK_Courses_Status_Range", "[Status] BETWEEN 1 AND 4");
            x.HasCheckConstraint("CK_Courses_DateRange", "[EndDate] >= [StartDate]");
        });
        builder.HasIndex(x => new { x.TeacherId, x.StartDate, x.EndDate }).HasDatabaseName("IX_Courses_Teacher_DateRange");
        builder.HasIndex(x => new { x.ClassroomId, x.StartDate, x.EndDate }).HasDatabaseName("IX_Courses_Classroom_DateRange");
        builder.HasIndex(x => new { x.BranchId, x.Status }).HasDatabaseName("IX_Courses_Branch_Status");
        builder.HasOne(x => x.Branch).WithMany(x => x.Courses).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OfferedLanguage).WithMany(x => x.Courses).HasForeignKey(x => x.OfferedLanguageId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CourseLevel).WithMany(x => x.Courses).HasForeignKey(x => x.CourseLevelId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Teacher).WithMany(x => x.Courses).HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Classroom).WithMany(x => x.Courses).HasForeignKey(x => x.ClassroomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.Branch.IsDeleted && !x.OfferedLanguage.IsDeleted && !x.CourseLevel.IsDeleted && !x.Teacher.IsDeleted && !x.Classroom.IsDeleted);
    }
}

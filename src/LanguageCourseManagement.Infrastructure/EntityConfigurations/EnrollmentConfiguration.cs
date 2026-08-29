using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.EnrollmentDate).HasColumnType("datetime2(0)");
        builder.Property(x => x.TuitionFee).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.FinalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.PaymentType).HasConversion<int>();
        builder.ToTable(x =>
        {
            x.HasCheckConstraint("CK_Enrollments_TuitionFee_NonNegative", "[TuitionFee] >= 0");
            x.HasCheckConstraint("CK_Enrollments_DiscountAmount_NonNegative", "[DiscountAmount] >= 0");
            x.HasCheckConstraint("CK_Enrollments_FinalAmount_NonNegative", "[FinalAmount] >= 0");
            x.HasCheckConstraint("CK_Enrollments_DiscountWithinTuition", "[DiscountAmount] <= [TuitionFee]");
            x.HasCheckConstraint("CK_Enrollments_FinalAmount_Calculation", "[FinalAmount] = [TuitionFee] - [DiscountAmount]");
            x.HasCheckConstraint("CK_Enrollments_Status_Range", "[Status] BETWEEN 1 AND 3");
            x.HasCheckConstraint("CK_Enrollments_PaymentType_Range", "[PaymentType] BETWEEN 1 AND 2");
        });
        builder.HasIndex(x => new { x.StudentId, x.CourseId }).HasDatabaseName("UX_Enrollments_Student_Course").IsUnique().HasFilter("[Status] != 3");
        builder.HasIndex(x => new { x.CourseId, x.Status }).HasDatabaseName("IX_Enrollments_Course_Status");
        // Composite index for status-first queries (e.g., listing active enrollments by course)
        builder.HasIndex(x => new { x.Status, x.CourseId }).HasDatabaseName("IX_Enrollments_Status_CourseId");
        builder.HasIndex(x => new { x.StudentId, x.Status }).HasDatabaseName("IX_Enrollments_Student_Status");
        builder.HasOne(x => x.Student).WithMany(x => x.Enrollments).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Course).WithMany(x => x.Enrollments).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted && !x.Student.IsDeleted && !x.Course.IsDeleted);
    }
}

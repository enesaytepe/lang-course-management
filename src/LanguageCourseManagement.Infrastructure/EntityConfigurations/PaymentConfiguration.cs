using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Method).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.PaymentDate).HasColumnType("datetime2(0)");
        builder.Property(x => x.SettledAt).HasColumnType("datetimeoffset(0)");
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.ToTable(x =>
        {
            x.HasCheckConstraint("CK_Payments_Amount_NonNegative", "[Amount] >= 0");
            x.HasCheckConstraint("CK_Payments_Method_Range", "[Method] BETWEEN 1 AND 3");
            x.HasCheckConstraint("CK_Payments_Status_Range", "[Status] BETWEEN 1 AND 4");
        });
        builder.HasIndex(x => x.EnrollmentId).HasDatabaseName("IX_Payments_Enrollment");
        builder.HasIndex(x => x.IdempotencyKey).HasDatabaseName("UX_Payments_IdempotencyKey").IsUnique();
        builder.HasOne(x => x.Enrollment).WithMany(x => x.Payments).HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Installment).WithMany(x => x.Payments).HasForeignKey(x => x.InstallmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.Enrollment.Student.IsDeleted && !x.Enrollment.Course.Branch.IsDeleted && !x.Enrollment.Course.OfferedLanguage.IsDeleted && !x.Enrollment.Course.CourseLevel.IsDeleted && !x.Enrollment.Course.Teacher.IsDeleted && !x.Enrollment.Course.Classroom.IsDeleted);
    }
}

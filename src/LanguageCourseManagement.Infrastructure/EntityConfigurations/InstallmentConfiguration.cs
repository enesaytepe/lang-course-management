using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class InstallmentConfiguration : IEntityTypeConfiguration<Installment>
{
    public void Configure(EntityTypeBuilder<Installment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.InstallmentNumber).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.DueDate).HasColumnType("date");
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.ToTable(x =>
        {
            x.HasCheckConstraint("CK_Installments_Amount_NonNegative", "[Amount] >= 0");
            x.HasCheckConstraint("CK_Installments_InstallmentNumber_Positive", "[InstallmentNumber] > 0");
            x.HasCheckConstraint("CK_Installments_Status_Range", "[Status] BETWEEN 1 AND 4");
        });
        builder.HasIndex(x => new { x.EnrollmentId, x.InstallmentNumber }).HasDatabaseName("UX_Installments_Enrollment_Number").IsUnique();
        builder.HasOne(x => x.Enrollment).WithMany(x => x.Installments).HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

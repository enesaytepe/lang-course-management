using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class TeacherLanguageConfiguration : IEntityTypeConfiguration<TeacherLanguage>
{
    public void Configure(EntityTypeBuilder<TeacherLanguage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.HasIndex(x => new { x.TeacherId, x.OfferedLanguageId }).HasDatabaseName("UX_TeacherLanguages_Teacher_Language").IsUnique();
        builder.HasOne(x => x.Teacher).WithMany(x => x.TeacherLanguages).HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OfferedLanguage).WithMany(x => x.TeacherLanguages).HasForeignKey(x => x.OfferedLanguageId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.Teacher.IsDeleted && !x.OfferedLanguage.IsDeleted);
    }
}

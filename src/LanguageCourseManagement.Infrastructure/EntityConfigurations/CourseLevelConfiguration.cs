using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class CourseLevelConfiguration : IEntityTypeConfiguration<CourseLevel>
{
    public void Configure(EntityTypeBuilder<CourseLevel> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnType("datetimeoffset(0)");
        builder.ToTable(x => x.HasCheckConstraint("CK_CourseLevels_Order_Positive", "[Order] > 0"));
        builder.HasIndex(x => new { x.OfferedLanguageId, x.Name }).HasDatabaseName("UX_CourseLevels_Language_Name_Active").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.OfferedLanguageId, x.Order }).HasDatabaseName("UX_CourseLevels_Language_Order_Active").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasOne(x => x.OfferedLanguage).WithMany(x => x.CourseLevels).HasForeignKey(x => x.OfferedLanguageId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted && !x.OfferedLanguage.IsDeleted);
    }
}

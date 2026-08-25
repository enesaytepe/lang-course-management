using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class OfferedLanguageConfiguration : IEntityTypeConfiguration<OfferedLanguage>
{
    public void Configure(EntityTypeBuilder<OfferedLanguage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(16);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnType("datetimeoffset(0)");
        builder.HasIndex(x => x.Name).HasDatabaseName("UX_OfferedLanguages_Name_Active").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.Code).HasDatabaseName("UX_OfferedLanguages_Code_Active").IsUnique().HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL");
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

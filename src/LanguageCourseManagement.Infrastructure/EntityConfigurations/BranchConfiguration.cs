using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500).IsRequired();
        builder.Property(x => x.PublicTransportationDirections).HasMaxLength(1000);
        builder.Property(x => x.PrivateVehicleDirections).HasMaxLength(1000);
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.Latitude).HasPrecision(9, 6);
        builder.Property(x => x.Longitude).HasPrecision(9, 6);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnType("datetimeoffset(0)");
        builder.ToTable(x =>
        {
            x.HasCheckConstraint("CK_Branches_Latitude_Range", "[Latitude] >= -90 AND [Latitude] <= 90");
            x.HasCheckConstraint("CK_Branches_Longitude_Range", "[Longitude] >= -180 AND [Longitude] <= 180");
        });
        builder.HasIndex(x => x.Name).HasDatabaseName("UX_Branches_Name_Active").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

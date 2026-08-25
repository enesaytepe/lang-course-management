using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class BranchFacilityConfiguration : IEntityTypeConfiguration<BranchFacility>
{
    public void Configure(EntityTypeBuilder<BranchFacility> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.HasIndex(x => new { x.BranchId, x.FacilityId }).HasDatabaseName("UX_BranchFacilities_Branch_Facility").IsUnique();
        builder.HasOne(x => x.Branch).WithMany(x => x.BranchFacilities).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Facility).WithMany(x => x.BranchFacilities).HasForeignKey(x => x.FacilityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.Branch.IsDeleted && !x.Facility.IsDeleted);
    }
}

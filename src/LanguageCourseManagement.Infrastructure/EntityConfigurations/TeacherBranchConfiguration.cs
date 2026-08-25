using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageCourseManagement.Infrastructure.EntityConfigurations;

public sealed class TeacherBranchConfiguration : IEntityTypeConfiguration<TeacherBranch>
{
    public void Configure(EntityTypeBuilder<TeacherBranch> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").IsRequired();
        builder.HasIndex(x => new { x.TeacherId, x.BranchId }).HasDatabaseName("UX_TeacherBranches_Teacher_Branch").IsUnique();
        builder.HasOne(x => x.Teacher).WithMany(x => x.TeacherBranches).HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany(x => x.TeacherBranches).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.Teacher.IsDeleted && !x.Branch.IsDeleted);
    }
}

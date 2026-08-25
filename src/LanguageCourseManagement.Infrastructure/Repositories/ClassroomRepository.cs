using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public sealed class ClassroomRepository
    : EfRepositoryBase<Classroom, AppDbContext>, IClassroomRepository
{
    public ClassroomRepository(AppDbContext context) : base(context)
    {
    }

    public Task<bool> NameExistsAsync(
        Guid branchId,
        string name,
        Guid? excludeClassroomId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        var query = Context.Classrooms
            .AsNoTracking()
            .Where(classroom =>
                classroom.BranchId == branchId &&
                classroom.Name.ToLower() == normalizedName);

        if (excludeClassroomId.HasValue)
            query = query.Where(classroom => classroom.Id != excludeClassroomId.Value);

        return query.AnyAsync(cancellationToken);
    }
}

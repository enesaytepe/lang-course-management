using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public sealed class CourseLevelRepository
    : EfRepositoryBase<CourseLevel, AppDbContext>, ICourseLevelRepository
{
    public CourseLevelRepository(AppDbContext context) : base(context)
    {
    }

    public Task<bool> NameExistsAsync(
        Guid offeredLanguageId,
        string name,
        Guid? excludeCourseLevelId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        var query = Context.CourseLevels
            .AsNoTracking()
            .Where(level =>
                level.OfferedLanguageId == offeredLanguageId &&
                level.Name.Trim().ToLower() == normalizedName);

        if (excludeCourseLevelId.HasValue)
            query = query.Where(level => level.Id != excludeCourseLevelId.Value);

        return query.AnyAsync(cancellationToken);
    }
}

using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public sealed class CourseRepository
    : EfRepositoryBase<Course, AppDbContext>, ICourseRepository
{
    public CourseRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<int> DeleteSchedulesByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var schedules = await Context.CourseSchedules
            .Where(s => s.CourseId == courseId)
            .ToListAsync(cancellationToken);

        if (schedules.Count == 0)
            return 0;

        Context.CourseSchedules.RemoveRange(schedules);
        return schedules.Count;
    }
}

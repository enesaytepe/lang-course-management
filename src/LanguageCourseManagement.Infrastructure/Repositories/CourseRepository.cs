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

    public Task<Course?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Context.Courses
            .Include(course => course.Branch)
            .Include(course => course.OfferedLanguage)
            .Include(course => course.CourseLevel)
            .Include(course => course.Teacher)
            .Include(course => course.Classroom)
            .Include(course => course.Schedules)
            .FirstOrDefaultAsync(course => course.Id == id, cancellationToken);
    }
}

using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public sealed class CourseRepository
    : EfRepositoryBase<Course, AppDbContext>, ICourseRepository
{
    public CourseRepository(AppDbContext context) : base(context)
    {
    }
}

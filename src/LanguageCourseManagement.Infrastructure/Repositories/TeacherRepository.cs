using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public sealed class TeacherRepository
    : EfRepositoryBase<Teacher, AppDbContext>, ITeacherRepository
{
    public TeacherRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Teacher?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Context.Teachers
            .Include(teacher => teacher.TeacherLanguages)
            .Include(teacher => teacher.TeacherBranches)
            .Include(teacher => teacher.Availabilities)
            .FirstOrDefaultAsync(teacher => teacher.Id == id, cancellationToken);
    }
}

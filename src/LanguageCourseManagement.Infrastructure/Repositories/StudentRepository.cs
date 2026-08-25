using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public sealed class StudentRepository
    : EfRepositoryBase<Student, AppDbContext>, IStudentRepository
{
    public StudentRepository(AppDbContext context) : base(context)
    {
    }
}

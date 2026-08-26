using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public sealed class EnrollmentRepository
    : EfRepositoryBase<Enrollment, AppDbContext>, IEnrollmentRepository
{
    public EnrollmentRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public Task<int> CountActiveByCourseIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return Context.Enrollments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(enrollment =>
                enrollment.CourseId == courseId &&
                enrollment.Status != EnrollmentStatus.Cancelled,
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<Enrollment?> FindByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return Context.Enrollments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                e => e.StudentId == studentId && e.CourseId == courseId,
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<Course?> GetCourseForSettlementAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return Context.Courses
            .FromSqlInterpolated($"SELECT * FROM [Courses] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {courseId}")
            .Include(c => c.Schedules)
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Student?> GetActiveStudentAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return Context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId && s.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Enrollment>> GetListWithIncludesAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Enrollment?> GetDetailsWithIncludesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.Payments)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}

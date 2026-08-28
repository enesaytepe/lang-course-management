using LanguageCourseManagement.Domain.DTOs;
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

    public Task<Teacher?> GetByIdWithDetailsForMutationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Context.Teachers
            .Include(teacher => teacher.TeacherLanguages)
            .Include(teacher => teacher.TeacherBranches)
            .Include(teacher => teacher.Availabilities)
            .FirstOrDefaultAsync(teacher => teacher.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Teacher>> GetEligibleTeachersAsync(
        Guid branchId,
        Guid offeredLanguageId,
        Guid courseLevelId,
        IReadOnlyList<ScheduleSlot> schedules,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludeCourseId = null,
        CancellationToken cancellationToken = default)
    {
        var schedulesList = schedules.ToList();

        return await Context.Teachers
            .AsNoTracking()
            .Where(teacher => teacher.IsActive)
            .Where(teacher => teacher.TeacherBranches != null &&
                teacher.TeacherBranches.Any(tb => tb.BranchId == branchId))
            .Where(teacher => teacher.TeacherLanguages != null &&
                teacher.TeacherLanguages.Any(tl => tl.OfferedLanguageId == offeredLanguageId))
            .Where(teacher => teacher.TeacherCourseLevels != null &&
                teacher.TeacherCourseLevels.Any(tcl => tcl.CourseLevelId == courseLevelId))
            .Where(teacher => schedulesList.All(s =>
                teacher.Availabilities != null && teacher.Availabilities.Any(a =>
                    a.DayOfWeek == s.DayOfWeek &&
                    a.StartTime <= s.StartTime &&
                    a.EndTime >= s.EndTime)))
            .Where(teacher => !teacher.Courses!.Any(c =>
                (!excludeCourseId.HasValue || c.Id != excludeCourseId.Value) &&
                c.StartDate <= endDate && c.EndDate >= startDate &&
                c.Schedules!.Any(cs =>
                    schedulesList.Any(s =>
                        cs.DayOfWeek == s.DayOfWeek &&
                        cs.StartTime < s.EndTime &&
                        cs.EndTime > s.StartTime))))
            .OrderBy(teacher => teacher.LastName).ThenBy(teacher => teacher.FirstName)
            .ToListAsync(cancellationToken);
    }
}

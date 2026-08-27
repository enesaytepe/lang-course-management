using LanguageCourseManagement.Domain.DTOs;
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

    public async Task<IReadOnlyList<Classroom>> GetEligibleClassroomsAsync(
        Guid branchId,
        IReadOnlyList<ScheduleSlot> schedules,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludeCourseId = null,
        CancellationToken cancellationToken = default)
    {
        var schedulesList = schedules.ToList();

        return await Context.Classrooms
            .AsNoTracking()
            .Where(classroom => classroom.BranchId == branchId && classroom.IsActive)
            .Where(classroom => !classroom.Courses!.Any(course =>
                (!excludeCourseId.HasValue || course.Id != excludeCourseId.Value) &&
                course.StartDate <= endDate && course.EndDate >= startDate &&
                course.Schedules!.Any(cs =>
                    schedulesList.Any(s =>
                        cs.DayOfWeek == s.DayOfWeek &&
                        cs.StartTime < s.EndTime &&
                        cs.EndTime > s.StartTime))))
            .OrderBy(classroom => classroom.Name)
            .ToListAsync(cancellationToken);
    }
}

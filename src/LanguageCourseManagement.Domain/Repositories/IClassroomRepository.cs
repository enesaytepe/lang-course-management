using LanguageCourseManagement.Domain.DTOs;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

public interface IClassroomRepository : IRepository<Classroom>
{
    Task<bool> NameExistsAsync(
        Guid branchId,
        string name,
        Guid? excludeClassroomId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verilen şube ve ders programına uygun derslikleri veritabanında filtreler.
    /// </summary>
    Task<IReadOnlyList<Classroom>> GetEligibleClassroomsAsync(
        Guid branchId,
        IReadOnlyList<ScheduleSlot> schedules,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludeCourseId = null,
        CancellationToken cancellationToken = default);
}
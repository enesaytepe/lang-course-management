using LanguageCourseManagement.Domain.DTOs;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Öğretmen veri erişim işlemlerini tanımlar.
/// </summary>
public interface ITeacherRepository : IRepository<Teacher>
{
    /// <summary>
    /// Öğretmeni dil, şube ve müsaitlik ilişkileriyle birlikte getirir.
    /// Bulunamazsa null döndürür.
    /// </summary>
    Task<Teacher?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verilen şube, dil ve ders programına uygun öğretmenleri veritabanında filtreler.
    /// </summary>
    Task<IReadOnlyList<Teacher>> GetEligibleTeachersAsync(
        Guid branchId,
        Guid offeredLanguageId,
        IReadOnlyList<ScheduleSlot> schedules,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludeCourseId = null,
        CancellationToken cancellationToken = default);
}

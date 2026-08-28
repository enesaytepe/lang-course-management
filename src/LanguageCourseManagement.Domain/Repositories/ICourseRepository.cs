using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Kurs veri erişim işlemlerini tanımlar.
/// </summary>
public interface ICourseRepository : IRepository<Course>
{
    /// <summary>
    /// Belirli bir kursa ait tüm ders programı kayıtlarını siler.
    /// Kurs silinirken owned lifecycle parçası olarak temizlik için kullanılır.
    /// </summary>
    Task<int> DeleteSchedulesByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
}

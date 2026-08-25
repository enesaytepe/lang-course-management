using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Kurs seviyesi veri erişim işlemlerini tanımlar.
/// </summary>
public interface ICourseLevelRepository : IRepository<CourseLevel>
{
    /// <summary>
    /// Aynı dil içinde belirtilen seviye adının başka bir seviyeye ait olup olmadığını kontrol eder.
    /// Güncelleme senaryosunda <paramref name="excludeCourseLevelId"/> ile ilgili seviye dışlanır.
    /// </summary>
    Task<bool> NameExistsAsync(Guid offeredLanguageId, string name, Guid? excludeCourseLevelId = null, CancellationToken cancellationToken = default);
}

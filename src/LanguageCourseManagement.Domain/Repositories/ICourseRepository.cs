using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Kurs veri erişim işlemlerini tanımlar.
/// </summary>
public interface ICourseRepository : IRepository<Course>
{
    /// <summary>
    /// Kursu şube, dil, seviye, öğretmen, derslik ve haftalık program ilişkileriyle birlikte getirir.
    /// Bulunamazsa null döndürür.
    /// </summary>
    Task<Course?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}

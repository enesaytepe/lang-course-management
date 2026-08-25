using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Dil veri erişim işlemlerini tanımlar.
/// </summary>
public interface IOfferedLanguageRepository : IRepository<OfferedLanguage>
{
    /// <summary>
    /// Belirtilen ismin başka bir dile ait olup olmadığını kontrol eder.
    /// Güncelleme senaryosunda <paramref name="excludeOfferedLanguageId"/> ile ilgili dil dışlanır.
    /// </summary>
    Task<bool> NameExistsAsync(string name, Guid? excludeOfferedLanguageId = null, CancellationToken cancellationToken = default);
}

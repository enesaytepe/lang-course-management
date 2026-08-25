using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Şube veri erişim işlemlerini tanımlar.
/// </summary>
public interface IBranchRepository : IRepository<Branch>
{
    /// <summary>
    /// Belirtilen ismin başka bir şubeye ait olup olmadığını kontrol eder.
    /// Güncelleme senaryosunda <paramref name="excludeBranchId"/> ile ilgili şube dışlanır.
    /// </summary>
    Task<bool> NameExistsAsync(string name, Guid? excludeBranchId = null);

    Task<Branch> UpdateWithFacilitiesAsync(Branch branch, List<Guid> facilityIds, CancellationToken cancellationToken = default);
}

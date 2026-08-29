using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Paging;

namespace LanguageCourseManagement.Domain.Repositories;

public interface IFacilityRepository
{
    Task<List<Facility>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Facility>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<List<Guid>> GetIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetActiveIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);

    Task<IPaginate<Facility>> GetListAsync(
        int index,
        int size,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<Facility?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string name,
        Guid? excludeFacilityId = null,
        CancellationToken cancellationToken = default);

    Task<Facility> AddAsync(
        Facility facility,
        CancellationToken cancellationToken = default);

    Task<Facility> UpdateAsync(
        Facility facility,
        CancellationToken cancellationToken = default);

    Task<Facility> DeleteAsync(
        Facility facility,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

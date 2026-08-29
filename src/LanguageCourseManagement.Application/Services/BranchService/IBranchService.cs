using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Branches;

namespace LanguageCourseManagement.Application.Services.BranchService;

/// <summary>
/// Şube CRUD işlemlerini tanımlar.
/// </summary>
public interface IBranchService
{
    /// <summary>
    /// ID'ye göre şubeyi getirir.
    /// </summary>
    Task<BranchResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Arama ve filtre kriterlerine göre şubeleri sayfalamalı getirir.
    /// </summary>
    Task<GetListResponse<BranchListResponse>> GetListAsync(PageRequest pageRequest, string? search, bool? isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni şube oluşturur.
    /// </summary>
    Task<BranchResponse> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut şubeyi günceller.
    /// </summary>
    Task<BranchResponse> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Şubeyi soft delete ile siler.
    /// </summary>
    Task<BranchResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Şubeyi derslik, kurs ve öğretmen listeleriyle birlikte getirir.
    /// </summary>
    Task<BranchDetailsResponse> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}

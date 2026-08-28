using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.OfferedLanguages;

namespace LanguageCourseManagement.Application.Services.OfferedLanguageService;

/// <summary>
/// Dil işlemlerini tanımlar.
/// </summary>
public interface IOfferedLanguageService
{
    /// <summary>
    /// ID'ye göre sunulan dili getirir.
    /// </summary>
    Task<OfferedLanguageResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Arama ve filtre kriterlerine göre dilleri sayfalamalı getirir.
    /// </summary>
    Task<GetListResponse<OfferedLanguageListResponse>> GetListAsync(PageRequest pageRequest, string? search, bool? isActive, bool showDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni sunulan dil oluşturur.
    /// </summary>
    Task<OfferedLanguageResponse> CreateAsync(CreateOfferedLanguageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut sunulan dili günceller.
    /// </summary>
    Task<OfferedLanguageResponse> UpdateAsync(Guid id, UpdateOfferedLanguageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sunulan dili soft delete ile siler.
    /// </summary>
    Task<OfferedLanguageResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

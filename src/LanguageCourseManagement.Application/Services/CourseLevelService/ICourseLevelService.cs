using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.CourseLevels;

namespace LanguageCourseManagement.Application.Services.CourseLevelService;

/// <summary>
/// Kurs seviyesi işlemlerini tanımlar.
/// </summary>
public interface ICourseLevelService
{
    /// <summary>
    /// ID'ye göre kurs seviyesini getirir.
    /// </summary>
    Task<CourseLevelResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Arama ve filtre kriterlerine göre kurs seviyelerini sayfalamalı getirir.
    /// </summary>
    Task<GetListResponse<CourseLevelListResponse>> GetListAsync(PageRequest pageRequest, string? search, Guid? offeredLanguageId, bool? isActive, bool showDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni kurs seviyesi oluşturur.
    /// </summary>
    Task<CourseLevelResponse> CreateAsync(CreateCourseLevelRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut kurs seviyesini günceller.
    /// </summary>
    Task<CourseLevelResponse> UpdateAsync(Guid id, UpdateCourseLevelRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kurs seviyesini soft delete ile siler.
    /// </summary>
    Task<CourseLevelResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

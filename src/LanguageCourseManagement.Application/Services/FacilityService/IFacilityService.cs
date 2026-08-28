using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Facilities;

namespace LanguageCourseManagement.Application.Services.FacilityService;

/// <summary>
/// Tesis (kep) işlemlerini tanımlar.
/// </summary>
public interface IFacilityService
{
    /// <summary>
    /// Tüm tesisleri getirir.
    /// </summary>
    Task<List<FacilityResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Yalnızca aktif tesisleri getirir.
    /// </summary>
    Task<List<FacilityResponse>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// ID'ye göre tesisi getirir.
    /// </summary>
    Task<FacilityResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Arama ve filtre kriterlerine göre tesisleri sayfalamalı getirir.
    /// </summary>
    Task<GetListResponse<FacilityListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        bool showDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni tesis oluşturur.
    /// </summary>
    Task<FacilityResponse> CreateAsync(
        CreateFacilityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut tesisi günceller.
    /// </summary>
    Task<FacilityResponse> UpdateAsync(
        Guid id,
        UpdateFacilityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tesisi soft delete ile siler.
    /// </summary>
    Task<FacilityResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

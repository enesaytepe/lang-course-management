using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.OfferedLanguages;

namespace LanguageCourseManagement.Application.Services.OfferedLanguageService;

/// <summary>
/// Dil işlemlerini tanımlar.
/// </summary>
public interface IOfferedLanguageService
{
    Task<OfferedLanguageResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetListResponse<OfferedLanguageListResponse>> GetListAsync(PageRequest pageRequest, string? search, bool? isActive, bool showDeleted = false, CancellationToken cancellationToken = default);
    Task<OfferedLanguageResponse> CreateAsync(CreateOfferedLanguageRequest request, CancellationToken cancellationToken = default);
    Task<OfferedLanguageResponse> UpdateAsync(Guid id, UpdateOfferedLanguageRequest request, CancellationToken cancellationToken = default);
    Task<OfferedLanguageResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.CourseLevels;

namespace LanguageCourseManagement.Application.Services.CourseLevelService;

/// <summary>
/// Kurs seviyesi işlemlerini tanımlar.
/// </summary>
public interface ICourseLevelService
{
    Task<CourseLevelResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetListResponse<CourseLevelListResponse>> GetListAsync(PageRequest pageRequest, string? search, Guid? offeredLanguageId, bool? isActive, CancellationToken cancellationToken = default);
    Task<CourseLevelResponse> CreateAsync(CreateCourseLevelRequest request, CancellationToken cancellationToken = default);
    Task<CourseLevelResponse> UpdateAsync(Guid id, UpdateCourseLevelRequest request, CancellationToken cancellationToken = default);
    Task<CourseLevelResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

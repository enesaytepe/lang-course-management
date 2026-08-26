using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Teachers;

namespace LanguageCourseManagement.Application.Services.TeacherService;

/// <summary>
/// Öğretmen işlemlerini tanımlar.
/// </summary>
public interface ITeacherService
{
    Task<TeacherResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetListResponse<TeacherListResponse>> GetListAsync(PageRequest pageRequest, string? search, bool? isActive, bool showDeleted = false, CancellationToken cancellationToken = default);
    Task<TeacherResponse> CreateAsync(CreateTeacherRequest request, CancellationToken cancellationToken = default);
    Task<TeacherResponse> UpdateAsync(Guid id, UpdateTeacherRequest request, CancellationToken cancellationToken = default);
    Task<TeacherResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TeacherAvailabilityResponse> AddAvailabilityAsync(Guid teacherId, CreateTeacherAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task<TeacherAvailabilityResponse> UpdateAvailabilityAsync(Guid teacherId, Guid availabilityId, UpdateTeacherAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task<TeacherAvailabilityResponse> DeleteAvailabilityAsync(Guid teacherId, Guid availabilityId, CancellationToken cancellationToken = default);
}

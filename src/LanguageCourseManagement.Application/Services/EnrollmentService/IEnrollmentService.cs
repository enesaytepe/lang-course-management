using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.Services.EnrollmentService;
public interface IEnrollmentService
{
    Task<IReadOnlyList<EnrollmentListItemResponse>> GetListAsync(CancellationToken cancellationToken = default);
    Task<GetListResponse<EnrollmentListItemResponse>> GetListAsync(PageRequest pageRequest, string? search, Guid? branchId, EnrollmentStatus? status, bool showDeleted = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EnrollmentListItemResponse>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<EnrollmentDetailResponse> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EnrollmentDetailResponse> UpdateStatusAsync(Guid id, UpdateEnrollmentRequest request, CancellationToken cancellationToken = default);
    Task<EnrollmentDetailResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Öğrencinin belirli bir derse kayıt için uygun olup olmadığını kontrol eder.
    /// </summary>
    Task<EnrollmentEligibilityResponse> CheckEligibilityAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default);
}

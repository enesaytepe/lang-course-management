using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.DTOs.Payments;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.Services.EnrollmentService;
public interface IEnrollmentService
{
    Task<EnrollmentDetailResponse> RegisterAndSettleAsync(EnrollmentCreateRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EnrollmentListItemResponse>> GetListAsync(CancellationToken cancellationToken = default);
    Task<GetListResponse<EnrollmentListItemResponse>> GetListAsync(PageRequest pageRequest, string? search, Guid? branchId, EnrollmentStatus? status, bool showDeleted = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EnrollmentListItemResponse>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<EnrollmentDetailResponse> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EnrollmentDetailResponse> UpdateStatusAsync(Guid id, UpdateEnrollmentRequest request, CancellationToken cancellationToken = default);
    Task<EnrollmentDetailResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SettlementResponse>> GetPaymentsAsync(CancellationToken cancellationToken = default);
    Task<SettlementResponse> GetPaymentDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}

using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.Services.EnrollmentService;

/// <summary>
/// Kayıt (enrollment) işlemlerini tanımlar.
/// </summary>
public interface IEnrollmentService
{
    /// <summary>
    /// Tüm kayıtları listeler.
    /// </summary>
    Task<IReadOnlyList<EnrollmentListItemResponse>> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Arama ve filtre kriterlerine göre kayıtları sayfalamalı getirir.
    /// </summary>
    Task<GetListResponse<EnrollmentListItemResponse>> GetListAsync(PageRequest pageRequest, string? search, Guid? branchId, EnrollmentStatus? status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Öğrenciye ait kayıtları listeler.
    /// </summary>
    Task<IReadOnlyList<EnrollmentListItemResponse>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kaydın detay bilgilerini getirir.
    /// </summary>
    Task<EnrollmentDetailResponse> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kaydın durumunu günceller (ör. aktif, iptal).
    /// </summary>
    Task<EnrollmentDetailResponse> UpdateStatusAsync(Guid id, UpdateEnrollmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kaydı iptal eder.
    /// </summary>
    Task<EnrollmentDetailResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Öğrencinin belirli bir derse kayıt için uygun olup olmadığını kontrol eder.
    /// </summary>
    Task<EnrollmentEligibilityResponse> CheckEligibilityAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default);
}

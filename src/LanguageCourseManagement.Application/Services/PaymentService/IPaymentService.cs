using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.DTOs.Payments;
using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.Services.PaymentService;

/// <summary>
/// Tahsilat ve ödeme işlemlerini tanımlar.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Kayıt oluşturur ve ilk tahsilatı (nakit) gerçekleştirir.
    /// </summary>
    Task<EnrollmentDetailResponse> EnrollWithPaymentAsync(EnrollmentCreateRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tüm tahsilatları getirir (dashboard için).
    /// </summary>
    Task<IReadOnlyList<SettlementResponse>> GetAllSettlementsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// ID'ye göre tahsilatı getirir.
    /// </summary>
    Task<PaymentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Arama kriterlerine göre tahsilatları sayfalamalı getirir.
    /// </summary>
    Task<GetListResponse<PaymentListResponse>> GetListAsync(PageRequest pageRequest, string? search, Guid? branchId = null, PaymentStatus? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Nakit tam tahsilat gerçekleştirir.
    /// </summary>
    Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Öğrenciye ait tüm tahsilatları getirir (ödeme geçmişi için).
    /// </summary>
    Task<IReadOnlyList<PaymentHistoryItem>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Henüz tahsil edilmemiş aktif kayıtları getirir (tahsilat formu dropdown için).
    /// </summary>
    Task<IReadOnlyList<EnrollmentOptionDto>> GetUnsettledEnrollmentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen kayıtlara ait bekleyen taksitleri toplu olarak getirir (N+1 sorgusunu önler).
    /// </summary>
    Task<IReadOnlyList<InstallmentOptionDto>> GetPendingInstallmentsByEnrollmentIdsAsync(IReadOnlyList<Guid> enrollmentIds, CancellationToken cancellationToken = default);
}

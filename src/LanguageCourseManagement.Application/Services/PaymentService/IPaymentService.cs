using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.DTOs.Payments;

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
    Task<GetListResponse<PaymentListResponse>> GetListAsync(PageRequest pageRequest, string? search, Guid? branchId = null, bool showDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Nakit tam tahsilat gerçekleştirir.
    /// </summary>
    Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Öğrenciye ait tüm tahsilatları getirir (ödeme geçmişi için).
    /// </summary>
    Task<IReadOnlyList<PaymentHistoryItem>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
}

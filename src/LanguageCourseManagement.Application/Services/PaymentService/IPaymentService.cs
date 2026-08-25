using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Payments;

namespace LanguageCourseManagement.Application.Services.PaymentService;

/// <summary>
/// Nakit tahsilat işlemlerini tanımlar.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// ID'ye göre tahsilatı getirir.
    /// </summary>
    Task<PaymentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Arama kriterlerine göre tahsilatları sayfalamalı getirir.
    /// </summary>
    Task<GetListResponse<PaymentListResponse>> GetListAsync(PageRequest pageRequest, string? search, CancellationToken cancellationToken = default);

    /// <summary>
    /// Nakit tam tahsilat gerçekleştirir.
    /// </summary>
    Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, Guid userId, CancellationToken cancellationToken = default);
}

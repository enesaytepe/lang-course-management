using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Nakit tahsilat veri erişim işlemlerini tanımlar.
/// </summary>
public interface IPaymentRepository : IRepository<Payment>
{
    /// <summary>
    /// Bir kayda ait tahsilatı getirir; kaydın tahsilat idempotency/duplicate kontrolünde kullanılır.
    /// Kaydın tahsilatı yoksa null döndürür.
    /// </summary>
    Task<Payment?> GetByEnrollmentIdAsync(Guid enrollmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// İdempotensi anahtarına göre tahsilatı ilişkili verilerle birlikte getirir.
    /// </summary>
    Task<Payment?> FindByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default);

}

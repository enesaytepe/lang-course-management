using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.DTOs.Payments;

/// <summary>
/// Yeni tahsilat oluşturma isteği.
/// </summary>
public sealed class CreatePaymentRequest
{
    /// <summary>
    /// Tahsilat yapılacak kayıt Id
    /// </summary>
    public Guid EnrollmentId { get; set; }

    /// <summary>
    /// Tahsilat yapılacak taksit Id (nakit ödemelerde null)
    /// </summary>
    public Guid? InstallmentId { get; set; }

    /// <summary>
    /// Tahsilat yöntemi
    /// </summary>
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;

    /// <summary>
    /// Tahsilat açıklaması (isteğe bağlı)
    /// </summary>
    public string? Description { get; set; }
}

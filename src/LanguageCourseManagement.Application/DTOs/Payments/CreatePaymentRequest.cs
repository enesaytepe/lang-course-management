namespace LanguageCourseManagement.Application.DTOs.Payments;

/// <summary>
/// Yeni nakit tahsilat oluşturma isteği. Tutar otomatik olarak kaydın nihai tutarına eşitlenir.
/// </summary>
public sealed class CreatePaymentRequest
{
    /// <summary>
    /// Tahsilat yapılacak kayıt Id
    /// </summary>
    public Guid EnrollmentId { get; set; }

    /// <summary>
    /// Tahsilat açıklaması (isteğe bağlı)
    /// </summary>
    public string? Description { get; set; }
}

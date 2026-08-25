using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Kayıt için nakit tahsilat
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>
    /// Tahsilatın ait olduğu kayıt Id
    /// </summary>
    public Guid EnrollmentId { get; set; }

    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// Kayıt için tahsil edilen tutar
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Kayıtta kullanılan tahsilat yöntemi
    /// </summary>
    public PaymentMethod Method { get; set; }

    /// <summary>
    /// Kayıt tahsilatının durumu
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Tahsilatın tamamlandığı tarih
    /// </summary>
    public DateTimeOffset SettledAt { get; set; }

    /// <summary>
    /// Tahsilatı alan kullanıcı Id
    /// </summary>
    public Guid CollectedByUserId { get; set; }

    /// <summary>
    /// Tahsilat açıklaması
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tahsilatın ait olduğu öğrenci kaydı
    /// </summary>
    public Enrollment Enrollment { get; set; } = null!;
}

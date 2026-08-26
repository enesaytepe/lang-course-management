using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Kayıt için tahsilat kaydı
/// </summary>
public class Payment : SoftDeletableEntity
{
    /// <summary>
    /// Tahsilatın ait olduğu kayıt Id
    /// </summary>
    public Guid EnrollmentId { get; set; }

    /// <summary>
    /// Tahsilatın ait olduğu taksit Id (nakit ödemelerde null)
    /// </summary>
    public Guid? InstallmentId { get; set; }

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
    /// Tahsilatın yapıldığı tarih
    /// </summary>
    public DateTime PaymentDate { get; set; }

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
    public virtual Enrollment Enrollment { get; set; } = null!;

    /// <summary>
    /// Tahsilatın ait olduğu taksit
    /// </summary>
    public virtual Installment? Installment { get; set; }
}

using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Öğrenci kaydı
/// </summary>
public class Enrollment : SoftDeletableEntity
{
    /// <summary>
    /// Kaydı yapılan öğrenci Id
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    /// Kaydolunan ders Id
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// Kayıt tarihi
    /// </summary>
    public DateTime EnrollmentDate { get; set; }

    /// <summary>
    /// Kayıt anındaki kurs ücreti
    /// </summary>
    public decimal TuitionFee { get; set; }

    /// <summary>
    /// Kayıtta uygulanan indirim tutarı
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Kayıt için tahsil edilecek nihai tutar
    /// </summary>
    public decimal FinalAmount { get; set; }

    /// <summary>
    /// Kaydı oluşturan kullanıcı Id
    /// </summary>
    public Guid RegisteredByUserId { get; set; }

    /// <summary>
    /// Öğrenci kaydının durumu
    /// </summary>
    public EnrollmentStatus Status { get; set; }

    /// <summary>
    /// Ödeme türü: nakit tek seferde veya taksitli
    /// </summary>
    public PaymentType PaymentType { get; set; }

    /// <summary>
    /// Kaydı yapılan öğrenci
    /// </summary>
    public virtual Student Student { get; set; } = null!;

    /// <summary>
    /// Öğrencinin kaydolduğu ders
    /// </summary>
    public virtual Course Course { get; set; } = null!;

    /// <summary>
    /// Kayıt için oluşturulan tahsilatlar
    /// </summary>
    public virtual List<Payment>? Payments { get; set; }

    /// <summary>
    /// Kayıt için oluşturulan taksitler
    /// </summary>
    public virtual List<Installment>? Installments { get; set; }
}

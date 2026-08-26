using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Taksitli ödeme planının bir taksidi
/// </summary>
public class Installment : SoftDeletableEntity
{
    /// <summary>
    /// Taksidin ait olduğu kayıt Id
    /// </summary>
    public Guid EnrollmentId { get; set; }

    /// <summary>
    /// Taksit sırası (1, 2, 3, ...)
    /// </summary>
    public int InstallmentNumber { get; set; }

    /// <summary>
    /// Taksit tutarı
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Taksit vade tarihi
    /// </summary>
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Taksit durumu
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Taksit açıklaması
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Taksidin ait olduğu öğrenci kaydı
    /// </summary>
    public virtual Enrollment Enrollment { get; set; } = null!;

    /// <summary>
    /// Bu tansa ait tahsilatlar
    /// </summary>
    public virtual List<Payment>? Payments { get; set; }
}

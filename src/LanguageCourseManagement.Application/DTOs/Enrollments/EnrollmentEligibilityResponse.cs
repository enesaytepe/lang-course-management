namespace LanguageCourseManagement.Application.DTOs.Enrollments;

/// <summary>
/// Öğrencinin bir derse kayıt için uygunluk durumu.
/// </summary>
public sealed class EnrollmentEligibilityResponse
{
    /// <summary>
    /// Kayıt için uygun mu?
    /// </summary>
    public bool IsEligible { get; set; }

    /// <summary>
    /// Uygun değilse uyarı mesajı.
    /// </summary>
    public string? WarningMessage { get; set; }

    /// <summary>
    /// Zaten aktif kayıt varsa, mevcut kaydın Id'si.
    /// </summary>
    public Guid? ExistingEnrollmentId { get; set; }

    /// <summary>
    /// Dersteki mevcut kontenjan kullanımı.
    /// </summary>
    public int CurrentEnrollmentCount { get; set; }

    /// <summary>
    /// Dersin toplam kontenjanı.
    /// </summary>
    public int CourseCapacity { get; set; }
}

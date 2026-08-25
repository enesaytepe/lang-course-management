namespace LanguageCourseManagement.Domain.Enums;

/// <summary>
/// Öğrenci kaydının durumu
/// </summary>
public enum EnrollmentStatus
{
    /// <summary>
    /// Devam eden öğrenci kaydı
    /// </summary>
    Active = 1,
    /// <summary>
    /// Eğitimi tamamlanan öğrenci kaydı
    /// </summary>
    Completed = 2,
    /// <summary>
    /// İptal edilen öğrenci kaydı
    /// </summary>
    Cancelled = 3
}

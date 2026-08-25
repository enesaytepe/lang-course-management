namespace LanguageCourseManagement.Domain.Enums;

/// <summary>
/// Dersin açılış durumu
/// </summary>
public enum CourseStatus
{
    /// <summary>
    /// Henüz kayda açılmamış ders
    /// </summary>
    Draft = 1,
    /// <summary>
    /// Öğrenci kaydına açık ders
    /// </summary>
    Open = 2,
    /// <summary>
    /// Eğitim süreci tamamlanan ders
    /// </summary>
    Completed = 3,
    /// <summary>
    /// Açılışı iptal edilen ders
    /// </summary>
    Cancelled = 4
}

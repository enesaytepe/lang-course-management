namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Öğretmen müsaitliği
/// </summary>
public class TeacherAvailability : BaseEntity
{
    /// <summary>
    /// Müsaitliği tanımlanan öğretmen Id
    /// </summary>
    public Guid TeacherId { get; set; }

    /// <summary>
    /// Öğretmenin müsait olduğu hafta günü
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Müsaitlik başlangıç saati
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// Müsaitlik bitiş saati
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Müsaitliği tanımlanan öğretmen
    /// </summary>
    public Teacher Teacher { get; set; } = null!;
}

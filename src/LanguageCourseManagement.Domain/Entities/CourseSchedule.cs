namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Haftalık ders programı
/// </summary>
public class CourseSchedule : BaseEntity
{
    /// <summary>
    /// Programın ait olduğu ders Id
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// Dersin yapıldığı hafta günü
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Ders saatinin başlangıcı
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// Ders saatinin bitişi
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Programın ait olduğu ders
    /// </summary>
    public virtual Course Course { get; set; } = null!;
}

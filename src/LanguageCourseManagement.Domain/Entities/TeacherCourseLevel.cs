namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Öğretmenin verebildiği kurs seviyesi ilişkisi
/// </summary>
public class TeacherCourseLevel : BaseEntity
{
    /// <summary>
    /// Seviyeyi öğretebilen öğretmen Id
    /// </summary>
    public Guid TeacherId { get; set; }

    /// <summary>
    /// Öğretmenin öğretebildiği kurs seviyesi Id
    /// </summary>
    public Guid CourseLevelId { get; set; }

    /// <summary>
    /// Seviyeyi öğretebilen öğretmen
    /// </summary>
    public Teacher Teacher { get; set; } = null!;

    /// <summary>
    /// Öğretmenin öğretebildiği kurs seviyesi
    /// </summary>
    public CourseLevel CourseLevel { get; set; } = null!;
}

namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Öğretilen dil
/// </summary>
public class OfferedLanguage : SoftDeletableEntity
{
    /// <summary>
    /// Dil adı
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Dil kodu
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Dilin öğretime açık olma durumu
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Dil için tanımlanan kurs seviyeleri
    /// </summary>
    public List<CourseLevel>? CourseLevels { get; set; }

    /// <summary>
    /// Bu dili öğretebilen öğretmen ilişkileri
    /// </summary>
    public List<TeacherLanguage>? TeacherLanguages { get; set; }

    /// <summary>
    /// Bu dilde açılan dersler
    /// </summary>
    public List<Course>? Courses { get; set; }
}

namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Kurs seviyesi
/// </summary>
public class CourseLevel : SoftDeletableEntity
{
    /// <summary>
    /// Seviyenin ait olduğu dil Id
    /// </summary>
    public Guid OfferedLanguageId { get; set; }

    /// <summary>
    /// Seviye adı
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kurs seviyesi açıklaması
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Dil içindeki seviye sırası
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Kurs seviyesinin kullanım durumu
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Seviyenin ait olduğu dil
    /// </summary>
    public virtual OfferedLanguage OfferedLanguage { get; set; } = null!;

    /// <summary>
    /// Bu seviyede açılan dersler
    /// </summary>
    public virtual List<Course>? Courses { get; set; }

    /// <summary>
    /// Bu seviyeyi öğretebilen öğretmenler
    /// </summary>
    public virtual List<TeacherCourseLevel>? TeacherCourseLevels { get; set; }
}

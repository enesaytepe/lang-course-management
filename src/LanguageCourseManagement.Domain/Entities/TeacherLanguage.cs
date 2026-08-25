namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Öğretmen dil ilişkisi
/// </summary>
public class TeacherLanguage : BaseEntity
{
    /// <summary>
    /// Dili öğretebilen öğretmen Id
    /// </summary>
    public Guid TeacherId { get; set; }

    /// <summary>
    /// Öğretmenin öğretebildiği dil Id
    /// </summary>
    public Guid OfferedLanguageId { get; set; }

    /// <summary>
    /// Dili öğretebilen öğretmen
    /// </summary>
    public Teacher Teacher { get; set; } = null!;

    /// <summary>
    /// Öğretmenin öğretebildiği dil
    /// </summary>
    public OfferedLanguage OfferedLanguage { get; set; } = null!;
}

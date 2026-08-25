namespace LanguageCourseManagement.Application.DTOs.Enrollments;

/// <summary>
/// Kayıt arama/sezme yanıtı.
/// </summary>
public sealed class EnrollmentLookupResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

namespace LanguageCourseManagement.Application.DTOs.OfferedLanguages;

public sealed class OfferedLanguageListResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
}

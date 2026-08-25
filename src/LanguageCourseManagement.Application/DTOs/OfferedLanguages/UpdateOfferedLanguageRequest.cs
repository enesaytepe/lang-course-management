namespace LanguageCourseManagement.Application.DTOs.OfferedLanguages;

public sealed class UpdateOfferedLanguageRequest
{
    public required string Name { get; set; }
    public string? Code { get; set; }
    public bool IsActive { get; set; }
}

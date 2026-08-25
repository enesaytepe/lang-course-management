namespace LanguageCourseManagement.Application.DTOs.OfferedLanguages;

public sealed class CreateOfferedLanguageRequest
{
    public required string Name { get; set; }
    public string? Code { get; set; }
}

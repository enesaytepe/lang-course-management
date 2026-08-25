namespace LanguageCourseManagement.Application.DTOs.CourseLevels;

public sealed class CourseLevelResponse
{
    public Guid Id { get; set; }
    public Guid OfferedLanguageId { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
}

namespace LanguageCourseManagement.Application.DTOs.CourseLevels;

public sealed class CourseLevelListResponse
{
    public Guid Id { get; set; }
    public Guid OfferedLanguageId { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsActive { get; set; }
}

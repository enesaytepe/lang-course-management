namespace LanguageCourseManagement.Application.DTOs.CourseLevels;

public sealed class UpdateCourseLevelRequest
{
    public Guid OfferedLanguageId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
}

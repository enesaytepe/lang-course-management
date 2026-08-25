namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class CourseLevelDetailsViewModel
{
    public Guid Id { get; set; }
    public Guid OfferedLanguageId { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
}

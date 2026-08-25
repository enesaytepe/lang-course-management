namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class LanguageDetailsViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
}

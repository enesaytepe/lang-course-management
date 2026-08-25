namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class FacilityDetailsViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

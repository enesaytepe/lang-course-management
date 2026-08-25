namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class StudentDetailsViewModel
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? HomePhone { get; init; }
    public string MobilePhone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public DateTime RegistrationDate { get; init; }
    public bool IsActive { get; init; }
}

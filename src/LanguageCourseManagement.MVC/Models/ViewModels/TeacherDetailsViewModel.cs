namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class TeacherDetailsViewModel
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? HomePhone { get; init; }
    public string MobilePhone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public DateOnly HireDate { get; init; }
    public bool IsActive { get; init; }
    public List<string> Languages { get; init; } = [];
    public List<string> Branches { get; init; } = [];
    public List<string> CourseLevels { get; init; } = [];
    public List<TeacherAvailabilityDetailItem> Availabilities { get; init; } = [];
}

public sealed class TeacherAvailabilityDetailItem
{
    public string DayName { get; init; } = string.Empty;
    public string StartTime { get; init; } = string.Empty;
    public string EndTime { get; init; } = string.Empty;
}

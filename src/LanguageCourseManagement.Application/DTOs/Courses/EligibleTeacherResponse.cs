namespace LanguageCourseManagement.Application.DTOs.Courses;

public sealed class EligibleTeacherResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
}

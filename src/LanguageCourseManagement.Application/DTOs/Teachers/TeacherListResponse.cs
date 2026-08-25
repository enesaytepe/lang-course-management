namespace LanguageCourseManagement.Application.DTOs.Teachers;

public sealed class TeacherListResponse
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string MobilePhone { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; }
}

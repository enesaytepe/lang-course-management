namespace LanguageCourseManagement.Application.DTOs.Students;

public sealed class CreateStudentRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? HomePhone { get; set; }
    public required string MobilePhone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}

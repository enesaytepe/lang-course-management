namespace LanguageCourseManagement.Application.DTOs.Students;

public sealed class StudentListResponse
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string MobilePhone { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }
}

namespace LanguageCourseManagement.Application.DTOs.Teachers;

public sealed class CreateTeacherRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? HomePhone { get; set; }
    public required string MobilePhone { get; set; }
    public string? Email { get; set; }
    public DateOnly HireDate { get; set; }
    public List<Guid> LanguageIds { get; set; } = [];
    public List<Guid> BranchIds { get; set; } = [];
}

namespace LanguageCourseManagement.Application.DTOs.Branches;

public sealed class BranchListResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}
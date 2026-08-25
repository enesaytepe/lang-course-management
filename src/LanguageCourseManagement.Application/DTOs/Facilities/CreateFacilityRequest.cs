namespace LanguageCourseManagement.Application.DTOs.Facilities;

public sealed class CreateFacilityRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

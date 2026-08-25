namespace LanguageCourseManagement.Application.DTOs.Facilities;

public sealed class UpdateFacilityRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

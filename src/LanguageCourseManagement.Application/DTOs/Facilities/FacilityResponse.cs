namespace LanguageCourseManagement.Application.DTOs.Facilities;

public sealed class FacilityResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

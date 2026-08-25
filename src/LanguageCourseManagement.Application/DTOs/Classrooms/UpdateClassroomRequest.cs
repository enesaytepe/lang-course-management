namespace LanguageCourseManagement.Application.DTOs.Classrooms;

public sealed class UpdateClassroomRequest
{
    public Guid BranchId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
}
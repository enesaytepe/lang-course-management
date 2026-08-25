namespace LanguageCourseManagement.Application.DTOs.Classrooms;

public sealed class CreateClassroomRequest
{
    public Guid BranchId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
}
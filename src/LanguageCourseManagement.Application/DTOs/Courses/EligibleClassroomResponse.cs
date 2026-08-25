namespace LanguageCourseManagement.Application.DTOs.Courses;

public sealed class EligibleClassroomResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

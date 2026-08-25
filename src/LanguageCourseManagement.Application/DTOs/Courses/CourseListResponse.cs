using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.DTOs.Courses;

public sealed class CourseListResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public string LevelName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string ClassroomName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Capacity { get; set; }
    public decimal TuitionFee { get; set; }
    public CourseStatus Status { get; set; }
    public bool IsActive { get; set; }
}

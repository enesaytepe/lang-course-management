using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.DTOs.Courses;

public sealed class CourseResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid OfferedLanguageId { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public Guid CourseLevelId { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public Guid ClassroomId { get; set; }
    public string ClassroomName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Capacity { get; set; }
    public decimal TuitionFee { get; set; }
    public CourseStatus Status { get; set; }
    public bool IsActive { get; set; }
}

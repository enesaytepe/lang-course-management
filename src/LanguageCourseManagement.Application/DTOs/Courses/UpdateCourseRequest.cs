using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.DTOs.Courses;

public sealed class UpdateCourseRequest
{
    public required string Name { get; set; }
    public Guid BranchId { get; set; }
    public Guid OfferedLanguageId { get; set; }
    public Guid CourseLevelId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid ClassroomId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Capacity { get; set; }
    public decimal TuitionFee { get; set; }
    public CourseStatus Status { get; set; }
    public bool IsActive { get; set; }
    public List<CourseScheduleItemDto> Schedules { get; set; } = [];
}

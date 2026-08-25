namespace LanguageCourseManagement.Application.DTOs.Courses;

public sealed class GetEligibleTeachersRequest
{
    public Guid BranchId { get; set; }
    public Guid OfferedLanguageId { get; set; }
    public Guid CourseLevelId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public List<CourseScheduleItemDto> Schedules { get; set; } = [];
    public Guid? ExcludeCourseId { get; set; }
}

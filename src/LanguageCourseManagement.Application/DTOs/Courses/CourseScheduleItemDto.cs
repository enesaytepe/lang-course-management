namespace LanguageCourseManagement.Application.DTOs.Courses;

public sealed class CourseScheduleItemDto
{
    public Guid? Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

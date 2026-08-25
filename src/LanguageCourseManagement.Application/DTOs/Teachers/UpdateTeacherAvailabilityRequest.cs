namespace LanguageCourseManagement.Application.DTOs.Teachers;

public sealed class UpdateTeacherAvailabilityRequest
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

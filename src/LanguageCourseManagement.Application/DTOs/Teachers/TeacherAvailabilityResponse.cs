namespace LanguageCourseManagement.Application.DTOs.Teachers;

public sealed class TeacherAvailabilityResponse
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

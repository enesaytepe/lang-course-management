namespace LanguageCourseManagement.Application.DTOs.Schedules;

public sealed class WeeklyScheduleResponse
{
    public string CourseName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? TeacherName { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int StudentCount { get; set; }
}

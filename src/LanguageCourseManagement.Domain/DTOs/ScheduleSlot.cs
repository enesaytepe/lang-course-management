namespace LanguageCourseManagement.Domain.DTOs;

public record ScheduleSlot(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

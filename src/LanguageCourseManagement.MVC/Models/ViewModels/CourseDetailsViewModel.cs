using LanguageCourseManagement.Application.DTOs.Courses;
using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class CourseDetailsViewModel
{
    public Guid Id { get; set; } public string Name { get; set; } = string.Empty;
    public Guid BranchId { get; set; } public string BranchName { get; set; } = string.Empty;
    public Guid OfferedLanguageId { get; set; } public string LanguageName { get; set; } = string.Empty;
    public Guid CourseLevelId { get; set; } public string LevelName { get; set; } = string.Empty;
    public Guid TeacherId { get; set; } public string TeacherName { get; set; } = string.Empty;
    public Guid ClassroomId { get; set; } public string ClassroomName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; } public DateOnly EndDate { get; set; }
    public int Capacity { get; set; } public decimal TuitionFee { get; set; }
    public CourseStatus Status { get; set; } public bool IsActive { get; set; }
    public IReadOnlyList<CourseScheduleItemDto> Schedules { get; set; } = [];
    public static CourseDetailsViewModel FromResponse(CourseResponse response, IReadOnlyList<CourseScheduleItemDto> schedules)
    {
        return new()
        {
            Id = response.Id,
            Name = response.Name,
            BranchId = response.BranchId,
            BranchName = response.BranchName,
            OfferedLanguageId = response.OfferedLanguageId,
            LanguageName = response.LanguageName,
            CourseLevelId = response.CourseLevelId,
            LevelName = response.LevelName,
            TeacherId = response.TeacherId,
            TeacherName = response.TeacherName,
            ClassroomId = response.ClassroomId,
            ClassroomName = response.ClassroomName,
            StartDate = response.StartDate,
            EndDate = response.EndDate,
            Capacity = response.Capacity,
            TuitionFee = response.TuitionFee,
            Status = response.Status,
            IsActive = response.IsActive,
            Schedules = schedules
        };
    }
}

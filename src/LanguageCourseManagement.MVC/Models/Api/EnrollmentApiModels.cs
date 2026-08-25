using LanguageCourseManagement.Application.DTOs.Enrollments;

namespace LanguageCourseManagement.MVC.Models.Api;
public sealed class EnrollmentCreateApiModel
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public decimal DiscountAmount { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}

public static class EnrollmentApiMapping
{
    public static EnrollmentCreateRequest ToRequest(this EnrollmentCreateApiModel model)
    {
        return new() { StudentId = model.StudentId, CourseId = model.CourseId, DiscountAmount = model.DiscountAmount, IdempotencyKey = model.IdempotencyKey };
    }
}

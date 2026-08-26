using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.DTOs.Enrollments;

public sealed class EnrollmentCreateRequest
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public decimal DiscountAmount { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public PaymentType PaymentType { get; set; } = PaymentType.Cash;
}

using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class EnrollmentCreateViewModel
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public decimal DiscountAmount { get; set; }
    public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString("N");
    public PaymentType PaymentType { get; set; } = PaymentType.Cash;
}

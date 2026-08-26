using LanguageCourseManagement.Application.DTOs.Enrollments;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class EnrollmentDetailViewModel
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public decimal TuitionFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public bool IsSettled { get; set; }
    public Guid? PaymentId { get; set; }
    public List<InstallmentResponse> Installments { get; set; } = [];
}

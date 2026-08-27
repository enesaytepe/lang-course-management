using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.DTOs.Payments;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class StudentDetailsViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? HomePhone { get; set; }
    public string MobilePhone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<EnrollmentListItemResponse> Enrollments { get; set; } = [];
    public IReadOnlyList<PaymentHistoryItem> PaymentHistory { get; set; } = [];
}

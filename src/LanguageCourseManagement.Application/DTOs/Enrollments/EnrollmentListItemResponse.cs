namespace LanguageCourseManagement.Application.DTOs.Enrollments;

/// <summary>
/// Kayıt listesindeki bir kaydın özet bilgisi.
/// </summary>
public sealed class EnrollmentListItemResponse
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsSettled { get; set; }
    public string PaymentType { get; set; } = string.Empty;
}

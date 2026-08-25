namespace LanguageCourseManagement.Application.DTOs.Payments;

/// <summary>
/// Tek bir tahsilatın detay bilgisini içeren yanıt.
/// </summary>
public sealed class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset SettledAt { get; set; }
    public string? Description { get; set; }
}

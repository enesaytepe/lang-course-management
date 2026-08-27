namespace LanguageCourseManagement.Application.DTOs.Payments;

/// <summary>
/// Öğrenci ödeme geçmişi kalemi.
/// </summary>
public sealed class PaymentHistoryItem
{
    public string CourseName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

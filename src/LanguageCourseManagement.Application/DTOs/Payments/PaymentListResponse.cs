namespace LanguageCourseManagement.Application.DTOs.Payments;

/// <summary>
/// Tahsilat listesindeki bir kaydın özet bilgisi.
/// </summary>
public sealed class PaymentListResponse
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset SettledAt { get; set; }
    public int? InstallmentNumber { get; set; }
}

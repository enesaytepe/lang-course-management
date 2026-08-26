namespace LanguageCourseManagement.Application.DTOs.Enrollments;

/// <summary>
/// Taksit bilgisi içeren yanıt.
/// </summary>
public sealed class InstallmentResponse
{
    public Guid Id { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PaidAmount { get; set; }
    public bool IsPaid { get; set; }
}

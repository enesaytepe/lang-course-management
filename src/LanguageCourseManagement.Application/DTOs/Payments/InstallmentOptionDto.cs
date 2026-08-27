namespace LanguageCourseManagement.Application.DTOs.Payments;

/// <summary>
/// Tahsilat formu için bekleyen taksit seçeneği.
/// </summary>
public sealed class InstallmentOptionDto
{
    public Guid EnrollmentId { get; init; }
    public Guid Id { get; init; }
    public int InstallmentNumber { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DueDate { get; init; }
    public string Status { get; init; } = string.Empty;
}

namespace LanguageCourseManagement.Application.DTOs.Payments;

/// <summary>
/// Tahsilat formu için henüz tahsil edilmemiş kayıt seçeneği.
/// </summary>
public sealed class EnrollmentOptionDto
{
    public Guid Id { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string CourseName { get; init; } = string.Empty;
    public string BranchName { get; init; } = string.Empty;
    public decimal FinalAmount { get; init; }
    public string PaymentType { get; init; } = string.Empty;
}

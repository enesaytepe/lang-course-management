namespace LanguageCourseManagement.Application.DTOs.Payments;

/// <summary>
/// Tek bir tahsilatın detay bilgisini içeren yanıt.
/// </summary>
public sealed class SettlementResponse
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset SettledAt { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}

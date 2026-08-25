namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class PaymentSettlementViewModel
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset SettledAt { get; set; }
}

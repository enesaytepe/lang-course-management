namespace LanguageCourseManagement.MVC.Models.Api;
public sealed record PaymentReadApiModel(Guid Id, Guid EnrollmentId, decimal Amount, string Method, string Status, DateTimeOffset SettledAt, string IdempotencyKey);

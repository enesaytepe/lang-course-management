using LanguageCourseManagement.Application.DTOs.Enrollments;

namespace LanguageCourseManagement.Application.Services.InstallmentService;

/// <summary>
/// Taksit işlemlerini tanımlar.
/// </summary>
public interface IInstallmentService
{
    /// <summary>
    /// Kayıt için taksit planı oluşturur.
    /// </summary>
    Task<IReadOnlyList<InstallmentResponse>> CreateInstallmentPlanAsync(Guid enrollmentId, int installmentCount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kaydın taksitlerini listeler.
    /// </summary>
    Task<IReadOnlyList<InstallmentResponse>> GetByEnrollmentIdAsync(Guid enrollmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vadesi geçmiş bekleyen taksitleri Overdue durumuna geçirir.
    /// </summary>
    Task MarkOverdueInstallmentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Overdue durumundaki taksit sayısını döndürür.
    /// </summary>
    Task<int> GetOverdueCountAsync(CancellationToken cancellationToken = default);
}

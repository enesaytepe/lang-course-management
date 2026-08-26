using LanguageCourseManagement.Application.DTOs.Dashboard;

namespace LanguageCourseManagement.Application.Services.DashboardService;

/// <summary>
/// Dashboard istatistik işlemlerini tanımlar.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Dashboard için gerekli tüm istatistikleri tek sorgu ile getirir.
    /// </summary>
    Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default);
}

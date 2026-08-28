using LanguageCourseManagement.Application.DTOs.Dashboard;

namespace LanguageCourseManagement.Application.Persistence;

/// <summary>
/// Dashboard verileri için toplu sorgulama işlemlerini tanımlar.
/// Tek bir aggregate sorgu ile tüm dashboard istatistiklerini çeker.
/// </summary>
public interface IDashboardRepository
{
    /// <summary>
    /// Dashboard için gerekli tüm istatistikleri tek sorgu ile getirir.
    /// </summary>
    Task<DashboardStatisticsResponse> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

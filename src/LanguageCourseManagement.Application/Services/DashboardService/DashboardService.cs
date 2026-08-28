using LanguageCourseManagement.Application.DTOs.Dashboard;
using LanguageCourseManagement.Application.Persistence;

namespace LanguageCourseManagement.Application.Services.DashboardService;

/// <inheritdoc />
public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    /// <inheritdoc />
    public Task<DashboardStatisticsResponse> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return _dashboardRepository.GetStatisticsAsync(cancellationToken);
    }
}

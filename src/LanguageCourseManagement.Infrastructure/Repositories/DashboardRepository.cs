using System.Data;
using LanguageCourseManagement.Application.DTOs.Dashboard;
using LanguageCourseManagement.Application.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LanguageCourseManagement.Infrastructure.Repositories;

/// <summary>
/// Dashboard istatistikleri için optimize edilmiş ham SQL sorgularını çalıştırır.
/// Tek bir aggregate sorgu ile N+1 sorununu çözer.
/// </summary>
public sealed class DashboardRepository : IDashboardRepository
{
    private readonly string _connectionString;

    public DashboardRepository(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException(
                "The required configuration key 'ConnectionStrings:DefaultConnection' is missing or empty.");
    }

    /// <inheritdoc />
    public async Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM Branches    WHERE IsDeleted = 0 AND IsActive = 1) AS ActiveBranchCount,
                (SELECT COUNT(*) FROM Classrooms  WHERE IsDeleted = 0 AND IsActive = 1) AS ActiveClassroomCount,
                (SELECT COUNT(*) FROM Teachers    WHERE IsDeleted = 0 AND IsActive = 1) AS ActiveTeacherCount,
                (SELECT COUNT(*) FROM Students    WHERE IsDeleted = 0 AND IsActive = 1) AS ActiveStudentCount,
                (SELECT COUNT(*) FROM Courses     WHERE IsDeleted = 0 AND IsActive = 1) AS ActiveCourseCount,
                (SELECT COUNT(*) FROM Enrollments WHERE IsDeleted = 0)                   AS TotalEnrollmentCount,
                (SELECT COUNT(*) FROM Enrollments WHERE IsDeleted = 0 AND Status = 1)   AS ActiveEnrollments,
                (SELECT ISNULL(SUM(Amount), 0) FROM Payments WHERE IsDeleted = 0 AND Status = 1) AS TotalSettledAmount,
                (SELECT ISNULL(SUM(Amount), 0) FROM Payments
                    WHERE IsDeleted = 0
                      AND Status = 1
                      AND YEAR(SettledAt) = YEAR(GETUTCDATE())
                      AND MONTH(SettledAt) = MONTH(GETUTCDATE())
                ) AS MonthlyRevenue,
                (SELECT COUNT(*) FROM Enrollments e
                    WHERE e.IsDeleted = 0
                      AND NOT EXISTS (
                          SELECT 1 FROM Payments p
                          WHERE p.EnrollmentId = e.Id AND p.IsDeleted = 0
                      )
                ) AS PendingPaymentCount,
                (SELECT COUNT(*) FROM Installments WHERE IsDeleted = 0 AND Status = 3) AS OverdueInstallmentCount
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection)
        {
            CommandType = CommandType.Text,
            CommandTimeout = 15
        };

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new DashboardStats
            {
                ActiveBranchCount = reader.GetInt32(0),
                ActiveClassroomCount = reader.GetInt32(1),
                ActiveTeacherCount = reader.GetInt32(2),
                ActiveStudentCount = reader.GetInt32(3),
                ActiveCourseCount = reader.GetInt32(4),
                TotalEnrollmentCount = reader.GetInt32(5),
                ActiveEnrollments = reader.GetInt32(6),
                TotalSettledAmount = reader.GetDecimal(7),
                MonthlyRevenue = reader.GetDecimal(8),
                PendingPaymentCount = reader.GetInt32(9),
                OverdueInstallmentCount = reader.GetInt32(10)
            };
        }

        return new DashboardStats();
    }
}

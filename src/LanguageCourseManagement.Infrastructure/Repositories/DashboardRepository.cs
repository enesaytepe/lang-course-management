using LanguageCourseManagement.Application.DTOs.Dashboard;
using LanguageCourseManagement.Application.Persistence;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LanguageCourseManagement.Infrastructure.Repositories;

/// <summary>
/// Dashboard istatistikleri için EF Core tabanlı toplu sorguları çalıştırır.
/// Tek bir aggregate sorgu ile N+1 sorununu çözer.
/// </summary>
public sealed class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<DashboardStatisticsResponse> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentYear = now.Year;
        var currentMonth = now.Month;

        var activeBranchCount = await _context.Branches
            .AsNoTracking()
            .CountAsync(b => b.IsActive, cancellationToken);

        var activeClassroomCount = await _context.Classrooms
            .AsNoTracking()
            .CountAsync(c => c.IsActive, cancellationToken);

        var activeTeacherCount = await _context.Teachers
            .AsNoTracking()
            .CountAsync(t => t.IsActive, cancellationToken);

        var activeStudentCount = await _context.Students
            .AsNoTracking()
            .CountAsync(s => s.IsActive, cancellationToken);

        var activeCourseCount = await _context.Courses
            .AsNoTracking()
            .CountAsync(c => c.IsActive, cancellationToken);

        var totalEnrollmentCount = await _context.Enrollments
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeEnrollments = await _context.Enrollments
            .AsNoTracking()
            .CountAsync(e => e.Status == EnrollmentStatus.Active, cancellationToken);

        var completedEnrollmentCount = await _context.Enrollments
            .AsNoTracking()
            .CountAsync(e => e.Status == EnrollmentStatus.Completed, cancellationToken);

        var cancelledEnrollmentCount = await _context.Enrollments
            .AsNoTracking()
            .CountAsync(e => e.Status == EnrollmentStatus.Cancelled, cancellationToken);

        var totalSettledAmount = await _context.Payments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Settled)
            .SumAsync(p => p.Amount, cancellationToken);

        var monthlyRevenue = await _context.Payments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Settled
                     && p.SettledAt.Year == currentYear
                     && p.SettledAt.Month == currentMonth)
            .SumAsync(p => p.Amount, cancellationToken);

        var pendingPaymentCount = await _context.Enrollments
            .AsNoTracking()
            .CountAsync(e => !_context.Payments.Any(p => p.EnrollmentId == e.Id), cancellationToken);

        var overdueInstallmentCount = await _context.Installments
            .AsNoTracking()
            .CountAsync(i => i.Status == PaymentStatus.Overdue, cancellationToken);

        var totalPaymentCount = await _context.Payments
            .AsNoTracking()
            .CountAsync(cancellationToken);

        return new DashboardStatisticsResponse
        {
            ActiveBranchCount = activeBranchCount,
            ActiveClassroomCount = activeClassroomCount,
            ActiveTeacherCount = activeTeacherCount,
            ActiveStudentCount = activeStudentCount,
            ActiveCourseCount = activeCourseCount,
            TotalEnrollmentCount = totalEnrollmentCount,
            ActiveEnrollments = activeEnrollments,
            CompletedEnrollmentCount = completedEnrollmentCount,
            CancelledEnrollmentCount = cancelledEnrollmentCount,
            TotalSettledAmount = totalSettledAmount,
            MonthlyRevenue = monthlyRevenue,
            PendingPaymentCount = pendingPaymentCount,
            OverdueInstallmentCount = overdueInstallmentCount,
            TotalPaymentCount = totalPaymentCount
        };
    }
}

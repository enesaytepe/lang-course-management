using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public sealed class PaymentRepository
    : EfRepositoryBase<Payment, AppDbContext>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public Task<Payment?> GetByEnrollmentIdAsync(
        Guid enrollmentId,
        CancellationToken cancellationToken = default)
    {
        return Context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(payment => payment.EnrollmentId == enrollmentId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Payment?> FindByIdempotencyKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return Context.Payments
            .AsNoTracking()
            .Include(p => p.Enrollment)
                .ThenInclude(e => e.Student)
            .Include(p => p.Enrollment)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(p => p.IdempotencyKey == key, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Payment>> GetListWithIncludesAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Payments
            .AsNoTracking()
            .Include(p => p.Enrollment)
                .ThenInclude(e => e.Student)
            .OrderByDescending(p => p.SettledAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Payment?> GetDetailsWithIncludesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Context.Payments
            .AsNoTracking()
            .Include(p => p.Enrollment)
                .ThenInclude(e => e.Student)
            .Include(p => p.Enrollment)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}

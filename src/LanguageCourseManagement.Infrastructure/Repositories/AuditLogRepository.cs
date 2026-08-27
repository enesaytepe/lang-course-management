using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using LanguageCourseManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetAsync(
        Expression<Func<AuditLog, bool>> predicate,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AuditLog> queryable = _context.AuditLogs;
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        return await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IPaginate<AuditLog>> GetListAsync(
        Expression<Func<AuditLog, bool>>? predicate = null,
        Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>? orderBy = null,
        int index = 0,
        int size = 10,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AuditLog> queryable = _context.AuditLogs;
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (predicate != null)
            queryable = queryable.Where(predicate);
        if (orderBy != null)
            return await orderBy(queryable).ToPaginateAsync(index, size, from: 0, cancellationToken);
        return await queryable.ToPaginateAsync(index, size, from: 0, cancellationToken);
    }
}

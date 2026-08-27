using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Paging;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Audit log veri erişim işlemlerini tanımlar.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>Koşula göre tek audit log kaydı getirir; bulunamazsa null döndürür.</summary>
    Task<AuditLog?> GetAsync(Expression<Func<AuditLog, bool>> predicate, bool enableTracking = true, CancellationToken cancellationToken = default);

    /// <summary>Koşula göre sayfalı audit log listesi getirir.</summary>
    Task<IPaginate<AuditLog>> GetListAsync(
        Expression<Func<AuditLog, bool>>? predicate = null,
        Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>? orderBy = null,
        int index = 0,
        int size = 10,
        bool enableTracking = true,
        CancellationToken cancellationToken = default);
}

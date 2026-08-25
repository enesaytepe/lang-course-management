using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Paging;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Domain.Interfaces;

/// <summary>
/// Generic CRUD repository arayüzü.
/// </summary>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>Entity sorgusu döndürür.</summary>
    IQueryable<TEntity> Query();

    /// <summary>Koşula göre tek entity getirir; bulunamazsa null döndürür.</summary>
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool enableTracking = true, CancellationToken cancellationToken = default);

    /// <summary>Koşula göre sayfalanmış entity listesi getirir.</summary>
    Task<IPaginate<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool enableTracking = true, CancellationToken cancellationToken = default);

    /// <summary>Koşula uyan herhangi bir kayıt var mı kontrol eder.</summary>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, bool enableTracking = true, CancellationToken cancellationToken = default);

    /// <summary>Yeni entity ekler ve kaydeder. İptal belirteci kayıt işlemini iptal eder.</summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    /// <summary>Birden fazla entity ekler ve kaydeder. İptal belirteci kayıt işlemini iptal eder.</summary>
    Task<IList<TEntity>> AddRangeAsync(IList<TEntity> entity, CancellationToken cancellationToken = default);
    /// <summary>Entity'yi günceller ve kaydeder. İptal belirteci kayıt işlemini iptal eder.</summary>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    /// <summary>Birden fazla entity'yi günceller ve kaydeder. İptal belirteci kayıt işlemini iptal eder.</summary>
    Task<IList<TEntity>> UpdateRangeAsync(IList<TEntity> entity, CancellationToken cancellationToken = default);
    /// <summary>Entity'yi siler ve kaydeder. İptal belirteci kayıt işlemini iptal eder.</summary>
    Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    /// <summary>Birden fazla entity'yi siler ve kaydeder. İptal belirteci kayıt işlemini iptal eder.</summary>
    Task<IList<TEntity>> DeleteRangeAsync(IList<TEntity> entity, CancellationToken cancellationToken = default);
}

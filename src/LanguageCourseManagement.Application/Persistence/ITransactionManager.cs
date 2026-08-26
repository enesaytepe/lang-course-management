namespace LanguageCourseManagement.Application.Persistence;

/// <summary>
/// Veritabanı işlemleri için transaction yönetimini tanımlar.
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Yeni bir transaction başlatır.
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut transaction'ı onaylar.
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut transaction'ı geri alır.
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Öğrenci kaydı veri erişim işlemlerini tanımlar.
/// </summary>
public interface IEnrollmentRepository : IRepository<Enrollment>
{
    /// <summary>
    /// Bir dersteki aktif öğrenci kayıt sayısını döndürür (kontenjan kontrolü için).
    /// </summary>
    Task<int> CountActiveByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir dersteki aktif öğrenci kayıt sayısını UPDLOCK ile kilitli olarak döndürür — kontenjan kontrolü için transaction-safe.
    /// </summary>
    Task<int> CountActiveByCourseIdForUpdateAsync(Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen öğrenci ve derse ait kaydı arar; idempotensi ve çift-kayıt kontrolünde kullanılır.
    /// </summary>
    Task<Enrollment?> FindByStudentAndCourseAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dersi kilitleyerek getirir (UPDLOCK, HOLDLOCK) — settlement transaction sırasında kullanılır.
    /// </summary>
    Task<Course?> GetCourseForSettlementAsync(Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktif öğrenciyi getirir.
    /// </summary>
    Task<Student?> GetActiveStudentAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kayıtları ilişkili verilerle birlikte listeler (nesne takibi kapalı).
    /// </summary>
    Task<IReadOnlyList<Enrollment>> GetListWithIncludesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Kaydı ilişkili verilerle birlikte getirir (nesne takibi kapalı).
    /// </summary>
    Task<Enrollment?> GetDetailsWithIncludesAsync(Guid id, CancellationToken cancellationToken = default);
}

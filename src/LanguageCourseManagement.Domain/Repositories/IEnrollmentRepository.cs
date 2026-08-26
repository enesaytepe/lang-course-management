using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
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
    /// Kayıt uygunluk kontrolü için kurs temel bilgilerini getirir (kontenjan, aktif durum).
    /// </summary>
    Task<CourseEligibilityInfo?> GetCourseEligibilityInfoAsync(Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen öğrencinin aktif diğer kayıtlarının ders programı bilgilerini getirir (çakışma kontrolü için).
    /// </summary>
    Task<IReadOnlyList<CourseScheduleInfo>> GetStudentActiveScheduleAsync(Guid studentId, Guid excludeCourseId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Kayıt uygunluk kontrolü için kurs temel bilgisi.
/// </summary>
public sealed class CourseEligibilityInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public CourseStatus Status { get; set; }
}

/// <summary>
/// Ders programı çakışma kontrolü için bilgi.
/// </summary>
public sealed class CourseScheduleInfo
{
    public Guid CourseId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

namespace LanguageCourseManagement.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard istatistikleri için toplu veri nesnesi.
/// Tek bir aggregate sorgu ile veritabanından çekilir.
/// </summary>
public sealed class DashboardStatisticsResponse
{
    /// <summary>
    /// Aktif şube sayısı
    /// </summary>
    public int ActiveBranchCount { get; init; }

    /// <summary>
    /// Aktif derslik sayısı
    /// </summary>
    public int ActiveClassroomCount { get; init; }

    /// <summary>
    /// Aktif öğretmen sayısı
    /// </summary>
    public int ActiveTeacherCount { get; init; }

    /// <summary>
    /// Aktif öğrenci sayısı
    /// </summary>
    public int ActiveStudentCount { get; init; }

    /// <summary>
    /// Aktif kurs sayısı
    /// </summary>
    public int ActiveCourseCount { get; init; }

    /// <summary>
    /// Toplam kayıt (enrollment) sayısı
    /// </summary>
    public int TotalEnrollmentCount { get; init; }

    /// <summary>
    /// Aktif kayıt (enrollment) sayısı
    /// </summary>
    public int ActiveEnrollments { get; init; }

    /// <summary>
    /// Tamamlanmış kayıt sayısı
    /// </summary>
    public int CompletedEnrollmentCount { get; init; }

    /// <summary>
    /// İptal edilmiş kayıt sayısı
    /// </summary>
    public int CancelledEnrollmentCount { get; init; }

    /// <summary>
    /// Tahsil edilmiş toplam tutar
    /// </summary>
    public decimal TotalSettledAmount { get; init; }

    /// <summary>
    /// Aylık gelir (geçerli ay içinde tahsil edilen tutar)
    /// </summary>
    public decimal MonthlyRevenue { get; init; }

    /// <summary>
    /// Ödemesi yapılmamış kayıt sayısı
    /// </summary>
    public int PendingPaymentCount { get; init; }

    /// <summary>
    /// Vadesi geçmiş taksit sayısı
    /// </summary>
    public int OverdueInstallmentCount { get; init; }

    /// <summary>
    /// Toplam ödeme sayısı
    /// </summary>
    public int TotalPaymentCount { get; init; }
}

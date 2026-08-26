namespace LanguageCourseManagement.Domain.Enums;

/// <summary>
/// Kayıt tahsilatının durumu
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Tam tutarı tahsil edilen kayıt
    /// </summary>
    Settled = 1,

    /// <summary>
    /// Tahsilatı bekleyen kayıt
    /// </summary>
    Pending = 2,

    /// <summary>
    /// Vadesi geçen tahsilat
    /// </summary>
    Overdue = 3,

    /// <summary>
    /// İptal edilen tahsilat
    /// </summary>
    Cancelled = 4
}

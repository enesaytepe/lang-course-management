using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Sistemdeki değişikliklerin kaydedildiği denetim günlüğü
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Benzersiz tanımlayıcı
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Değişikliğin uygulandığı entity adı
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Değişikliğin uygulandığı entity'nin ID'si
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Yapılan işlem türü
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// İşlemi yapan kullanıcının ID'si
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// İşlemi yapan kullanıcının adı
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// İşlemin gerçekleştiği zaman
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Değişiklik öncesi değerler (JSON formatında)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Değişiklik sonrası değerler (JSON formatında)
    /// </summary>
    public string? NewValues { get; set; }
}

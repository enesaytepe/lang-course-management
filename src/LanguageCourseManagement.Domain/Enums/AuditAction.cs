namespace LanguageCourseManagement.Domain.Enums;

/// <summary>
/// Audit log kaydı türü
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Yeni kayıt oluşturuldu
    /// </summary>
    Created = 1,

    /// <summary>
    /// Mevcut kayıt güncellendi
    /// </summary>
    Updated = 2,

    /// <summary>
    /// Kayıt silindi
    /// </summary>
    Deleted = 3
}

namespace LanguageCourseManagement.Domain.Interfaces;

/// <summary>
/// Soft-delete destekleyen entity'ler için işaretleyici arayüz.
/// </summary>
public interface ISoftDelete
{
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Silme zamanı
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }
}
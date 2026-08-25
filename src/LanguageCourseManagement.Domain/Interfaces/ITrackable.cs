namespace LanguageCourseManagement.Domain.Interfaces;

/// <summary>
/// Oluşturma ve güncelleme zamanlarını izleyen entity'ler için arayüz.
/// </summary>
public interface ITrackable
{
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Son güncelleme zamanı; hiç güncellenmemişse null.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
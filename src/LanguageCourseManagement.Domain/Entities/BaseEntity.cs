using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Entities;


/// <summary>
/// Tüm entity'ler için temel sınıf.
/// </summary>
public abstract class BaseEntity : ITrackable
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
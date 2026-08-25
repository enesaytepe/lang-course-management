using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Soft-delete destekleyen entity base class.
/// Sadece geri alma veya denetim gereksinimi olan entity'ler bu class'tan turemelidir.
/// Ornek: User, Role, Tenant, TenantOwnership
/// </summary>
public abstract class SoftDeletableEntity : BaseEntity, ISoftDelete
{
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }
}
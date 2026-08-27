using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.Services.AuditLogService;

/// <summary>
/// Audit log işlemlerini tanımlar.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Audit log kayıtlarını filtreli ve sayfalı olarak listeler.
    /// </summary>
    Task<GetListResponse<AuditLogListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? entityName,
        AuditAction? action,
        string? search,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ID'ye göre audit log kaydını getirir.
    /// </summary>
    Task<AuditLogResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Audit log listeleme yanıtı
/// </summary>
public class AuditLogListResponse
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Audit log detay yanıtı
/// </summary>
public class AuditLogResponse
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}

using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Application.Services.AuditLogService;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<GetListResponse<AuditLogListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? entityName,
        AuditAction? action,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var searchLower = normalizedSearch?.ToLowerInvariant();

        Expression<Func<Domain.Entities.AuditLog, bool>> predicate = log =>
            (!string.IsNullOrEmpty(entityName) || log.EntityName == entityName) &&
            (!action.HasValue || log.Action == action.Value) &&
            (searchLower == null ||
             log.EntityName.ToLower().Contains(searchLower) ||
             log.EntityId.ToLower().Contains(searchLower) ||
             (log.UserName != null && log.UserName.ToLower().Contains(searchLower)));

        var auditLogs = await _auditLogRepository.GetListAsync(
            predicate: predicate,
            orderBy: query => query.OrderByDescending(log => log.Timestamp),
            index: pageRequest.PageIndex,
            size: pageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);

        return new GetListResponse<AuditLogListResponse>
        {
            Index = auditLogs.Index,
            Size = auditLogs.Size,
            Count = auditLogs.Count,
            Pages = auditLogs.Pages,
            HasPrevious = auditLogs.HasPrevious,
            HasNext = auditLogs.HasNext,
            Items = auditLogs.Items.Select(log => new AuditLogListResponse
            {
                Id = log.Id,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Action = log.Action,
                UserName = log.UserName,
                Timestamp = log.Timestamp
            }).ToList()
        };
    }

    public async Task<AuditLogResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var auditLog = await _auditLogRepository.GetAsync(
            item => item.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken);

        if (auditLog is null)
            throw new NotFoundException("Audit log kaydı bulunamadı.");

        return new AuditLogResponse
        {
            Id = auditLog.Id,
            EntityName = auditLog.EntityName,
            EntityId = auditLog.EntityId,
            Action = auditLog.Action,
            UserId = auditLog.UserId,
            UserName = auditLog.UserName,
            Timestamp = auditLog.Timestamp,
            OldValues = auditLog.OldValues,
            NewValues = auditLog.NewValues
        };
    }
}

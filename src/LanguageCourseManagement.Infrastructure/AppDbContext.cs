using System.Security.Claims;
using System.Text.Json;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LanguageCourseManagement.Infrastructure.Identity;

namespace LanguageCourseManagement.Infrastructure;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _isSavingAuditLogs;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Branch> Branches { get; set; }
    public DbSet<BranchFacility> BranchFacilities { get; set; }
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseLevel> CourseLevels { get; set; }
    public DbSet<CourseSchedule> CourseSchedules { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Facility> Facilities { get; set; }
    public DbSet<OfferedLanguage> OfferedLanguages { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Installment> Installments { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<TeacherAvailability> TeacherAvailabilities { get; set; }
    public DbSet<TeacherBranch> TeacherBranches { get; set; }
    public DbSet<TeacherLanguage> TeacherLanguages { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global soft-delete query filter: ISoftDelete implemente eden tüm entity'lere otomatik uygula
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                // Generic metodu reflection ile oluştur — EF Core statik generic metot gerektirir
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    }

    private static void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : class, ISoftDelete
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        HandleSoftDelete();
        HandleTimestamps();
        EnsureSettledPaymentsAreImmutable();

        var auditEntries = !_isSavingAuditLogs ? CaptureAuditLogs() : [];

        var result = base.SaveChanges(acceptAllChangesOnSuccess);

        if (auditEntries.Count > 0)
        {
            _isSavingAuditLogs = true;
            AuditLogs.AddRange(auditEntries);
            base.SaveChanges(acceptAllChangesOnSuccess);
            _isSavingAuditLogs = false;
        }

        return result;
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        HandleSoftDelete();
        HandleTimestamps();
        EnsureSettledPaymentsAreImmutable();

        var auditEntries = !_isSavingAuditLogs ? CaptureAuditLogs() : [];

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        if (auditEntries.Count > 0)
        {
            _isSavingAuditLogs = true;
            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            _isSavingAuditLogs = false;
        }

        return result;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(true, cancellationToken);
    }

    private void HandleSoftDelete()
    {
        foreach (var entry in ChangeTracker.Entries<ISoftDelete>()
            .Where(e => e.State == EntityState.Deleted))
        {
            // Fiziksel silme yerine soft delete: Deleted -> Modified, IsDeleted = true
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTime.UtcNow;
        }
    }

    private void HandleTimestamps()
    {
        // ITrackable entity'lerde oluşturma/güncelleme zamanlarını otomatik doldur
        foreach (var entry in ChangeTracker.Entries<ITrackable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    private void EnsureSettledPaymentsAreImmutable()
    {
        foreach (var entry in ChangeTracker.Entries<Payment>())
        {
            if (entry.State is not (EntityState.Modified or EntityState.Deleted))
                continue;

            var originalStatus = (PaymentStatus)entry.OriginalValues["Status"]!;

            if (originalStatus is PaymentStatus.Settled or PaymentStatus.Cancelled)
                throw new InvalidOperationException(
                    $"Payment in '{originalStatus}' status is immutable and cannot be modified or deleted.");
        }
    }

    private List<AuditLog> CaptureAuditLogs()
    {
        ChangeTracker.DetectChanges();
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
            return [];

        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();
        var timestamp = DateTime.UtcNow;
        var auditLogs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;
            var entityId = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id")?.CurrentValue?.ToString() ?? string.Empty;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entityName,
                EntityId = entityId,
                Action = entry.State switch
                {
                    EntityState.Added => AuditAction.Created,
                    EntityState.Modified => AuditAction.Updated,
                    EntityState.Deleted => AuditAction.Deleted,
                    _ => AuditAction.Updated
                },
                UserId = userId,
                UserName = userName,
                Timestamp = timestamp
            };

            if (entry.State == EntityState.Deleted)
            {
                auditLog.OldValues = JsonSerializer.Serialize(entry.Properties.ToDictionary(
                    p => p.Metadata.Name,
                    p => p.OriginalValue));
            }
            else if (entry.State == EntityState.Modified)
            {
                var changedProperties = entry.Properties
                    .Where(p => p.IsModified && p.Metadata.Name != "Id" && p.Metadata.Name != "UpdatedAt")
                    .ToList();

                if (changedProperties.Count > 0)
                {
                    auditLog.OldValues = JsonSerializer.Serialize(changedProperties.ToDictionary(
                        p => p.Metadata.Name,
                        p => p.OriginalValue));
                    auditLog.NewValues = JsonSerializer.Serialize(changedProperties.ToDictionary(
                        p => p.Metadata.Name,
                        p => p.CurrentValue));
                }
            }
            else if (entry.State == EntityState.Added)
            {
                auditLog.NewValues = JsonSerializer.Serialize(entry.Properties
                    .Where(p => p.Metadata.Name != "Id" && p.Metadata.Name != "CreatedAt" && p.Metadata.Name != "UpdatedAt")
                    .ToDictionary(
                        p => p.Metadata.Name,
                        p => p.CurrentValue));
            }

            auditLogs.Add(auditLog);
        }

        return auditLogs;
    }

    private string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private string? GetCurrentUserName()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }
}

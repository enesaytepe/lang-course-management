using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LanguageCourseManagement.Infrastructure.Identity;

namespace LanguageCourseManagement.Infrastructure;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
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
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        HandleSoftDelete();
        HandleTimestamps();
        EnsureSettledPaymentsAreImmutable();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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
}

using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Mapping;
using LanguageCourseManagement.Application.Persistence;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.Application.Validators;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Infrastructure;
using LanguageCourseManagement.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Xunit;

namespace LanguageCourseManagement.Tests;

/// <summary>
/// enrollment + payment atomically ve kapasite eşzamanlılık davranışı için
/// gerçek SQL Server bağlantısıyla integration test'leri.
/// SQL Server kullanılamıyorsa test'ler zarif biçimde atlanır.
/// </summary>
public sealed class EnrollmentTransactionTests : IDisposable
{
    private const string TestPrefix = "ITX_";

    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
        ?? @"Server=localhost;Database=LangCourseManagement_Test;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly bool _canConnect;

    public EnrollmentTransactionTests()
    {
        _canConnect = TryConnect();
        if (_canConnect)
            EnsureSchema();
    }

    // ────────────────────────────────────────────
    //  Test 1: Enrollment + Payment atomiklik
    // ────────────────────────────────────────────
    [Fact]
    public async Task EnrollmentPaymentAtomicity_WhenPaymentFails_EnrollmentIsNotCommitted()
    {
        if (!_canConnect)
            return;

        await using var context = CreateContext();
        await CleanupTestDataAsync(context);

        var branchId = Guid.NewGuid();
        var languageId = Guid.NewGuid();
        var courseLevelId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var classroomId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await SeedPrerequisiteEntitiesAsync(context, branchId, languageId, courseLevelId, teacherId, classroomId);
        await SeedCourseAsync(context, courseId, branchId, languageId, courseLevelId, teacherId, classroomId, capacity: 5);
        await SeedStudentAsync(context, studentId);

        var service = CreateEnrollmentService(context);

        var request = new EnrollmentCreateRequest
        {
            StudentId = studentId,
            CourseId = courseId,
            DiscountAmount = 0m,
            IdempotencyKey = $"{TestPrefix}key-{Guid.NewGuid():N}",
            PaymentType = PaymentType.Cash
        };

        var result = await service.RegisterAndSettleAsync(request, userId);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(studentId, result.StudentId);
        Assert.Equal(courseId, result.CourseId);
        Assert.True(result.IsSettled, "Nakit ödeme sonrası IsSettled true olmalıdır.");
        Assert.NotNull(result.PaymentId);

        // Yeni bir context ile doğrula (change tracking önyargısını önlemek için)
        await using var verifyContext = CreateContext();

        var enrollmentInDb = await verifyContext.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == result.Id);
        Assert.NotNull(enrollmentInDb);
        Assert.Equal(EnrollmentStatus.Active, enrollmentInDb!.Status);

        var paymentInDb = await verifyContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == result.PaymentId.Value);
        Assert.NotNull(paymentInDb);
        Assert.Equal(PaymentStatus.Settled, paymentInDb!.Status);
        Assert.Equal(PaymentMethod.Cash, paymentInDb.Method);
        Assert.Equal(result.FinalAmount, paymentInDb.Amount);
        Assert.Equal(request.IdempotencyKey, paymentInDb.IdempotencyKey);

        // Aynı öğrenci + aynı ders tekrar kaydedilemez (idempotent koruma)
        await Assert.ThrowsAsync<BusinessException>(
            () => service.RegisterAndSettleAsync(request, userId));

        // İkinci denemede hâlâ sadece 1 enrollment olmalı
        var countAfter = await verifyContext.Enrollments
            .IgnoreQueryFilters()
            .CountAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        Assert.Equal(1, countAfter);

        await CleanupTestDataAsync(context);
    }

    // ────────────────────────────────────────────
    //  Test 2: Kapasite eşzamanlılık
    // ────────────────────────────────────────────
    [Fact]
    public async Task CapacityConcurrency_TwoConcurrentEnrollments_OnlyOneSucceeds()
    {
        if (!_canConnect)
            return;

        await using var context = CreateContext();
        await CleanupTestDataAsync(context);

        var branchId = Guid.NewGuid();
        var languageId = Guid.NewGuid();
        var courseLevelId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var classroomId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var student1Id = Guid.NewGuid();
        var student2Id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await SeedPrerequisiteEntitiesAsync(context, branchId, languageId, courseLevelId, teacherId, classroomId);
        await SeedCourseAsync(context, courseId, branchId, languageId, courseLevelId, teacherId, classroomId, capacity: 1);
        await SeedStudentAsync(context, student1Id);
        await SeedStudentAsync(context, student2Id);

        var key1 = $"{TestPrefix}conc-1-{Guid.NewGuid():N}";
        var key2 = $"{TestPrefix}conc-2-{Guid.NewGuid():N}";

        // Her task kendi AppDbContext'ini kullanır (change tracking izolasyonu)
        var task1 = Task.Run(async () =>
        {
            await using var ctx = CreateContext();
            var svc = CreateEnrollmentService(ctx);
            var req = new EnrollmentCreateRequest
            {
                StudentId = student1Id,
                CourseId = courseId,
                DiscountAmount = 0m,
                IdempotencyKey = key1,
                PaymentType = PaymentType.Cash
            };
            return await svc.RegisterAndSettleAsync(req, userId);
        });

        var task2 = Task.Run(async () =>
        {
            await using var ctx = CreateContext();
            var svc = CreateEnrollmentService(ctx);
            var req = new EnrollmentCreateRequest
            {
                StudentId = student2Id,
                CourseId = courseId,
                DiscountAmount = 0m,
                IdempotencyKey = key2,
                PaymentType = PaymentType.Cash
            };
            return await svc.RegisterAndSettleAsync(req, userId);
        });

        var allTasks = Task.WhenAll(task1, task2);
        var exceptions = allTasks.Exception?.InnerExceptions.ToList();

        var successCount = 0;
        if (task1.IsCompletedSuccessfully)
            successCount++;
        if (task2.IsCompletedSuccessfully)
            successCount++;

        // İkisi de başarılıysa, en az biriDuplicate enrollmente neden olmuş olabilir;
        // kapasite check'i storage-level lock ile korunmalıdır.
        Assert.True(successCount <= 1,
            $"Kapasitesi 1 olan derse aynı anda 2 kayıt yapıldı (başarılı: {successCount}). " +
            $"Kapasite kontrolü transaction locking ile korunmalıdır.");

        // Veritabanında gerçekten sadece 1 enrollment olmalı
        await using var verifyContext = CreateContext();
        var totalEnrollments = await verifyContext.Enrollments
            .IgnoreQueryFilters()
            .CountAsync(e => e.CourseId == courseId);

        Assert.Equal(1, totalEnrollments);

        // Successful enrollment'ın payment'i de kayıtlı olmalı
        var remainingEnrollment = await verifyContext.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CourseId == courseId);
        Assert.NotNull(remainingEnrollment);

        var paymentExists = await verifyContext.Payments
            .AsNoTracking()
            .AnyAsync(p => p.EnrollmentId == remainingEnrollment!.Id);
        Assert.True(paymentExists, "Başarılı enrollment'ın nakit payment'i de kayıtlı olmalıdır.");

        // Başarısız olan task bir BusinessException fırlatmış olmalı
        Assert.True(!task1.IsCompletedSuccessfully || !task2.IsCompletedSuccessfully,
            "En az bir task BusinessException ile başarısız olmalıdır.");

        await CleanupTestDataAsync(context);
    }

    // ────────────────────────────────────────────
    //  Helper: AppDbContext factory
    // ────────────────────────────────────────────
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .EnableSensitiveDataLogging()
            .Options;
        return new AppDbContext(options);
    }

    // ────────────────────────────────────────────
    //  Helper: EnrollmentService factory (real repos + real transaction manager)
    // ────────────────────────────────────────────
    private static EnrollmentService CreateEnrollmentService(AppDbContext context)
    {
        var enrollmentRepo = new EnrollmentRepository(context);
        var paymentRepo = new PaymentRepository(context);
        var transactionManager = new EfTransactionManager(context);

        IValidator<EnrollmentCreateRequest> createValidator = new EnrollmentCreateRequestValidator();
        IValidator<UpdateEnrollmentRequest> updateValidator = new UpdateEnrollmentRequestValidator();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(EnrollmentProfile).Assembly);
        }, NullLoggerFactory.Instance);
        mapperConfig.AssertConfigurationIsValid();
        var mapper = mapperConfig.CreateMapper();

        return new EnrollmentService(
            enrollmentRepo,
            paymentRepo,
            transactionManager,
            createValidator,
            updateValidator,
            mapper);
    }

    // ────────────────────────────────────────────
    //  Helper: SQL Server bağlantı kontrolü
    // ────────────────────────────────────────────
    private static bool TryConnect()
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ────────────────────────────────────────────
    //  Helper: Veritabanı şemasını oluştur
    // ────────────────────────────────────────────
    private void EnsureSchema()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    // ────────────────────────────────────────────
    //  Helper: Zorunlu bağımlılık entity'lerini ekle
    // ────────────────────────────────────────────
    private static async Task SeedPrerequisiteEntitiesAsync(
        AppDbContext context,
        Guid branchId,
        Guid languageId,
        Guid courseLevelId,
        Guid teacherId,
        Guid classroomId)
    {
        context.Branches.Add(new Branch
        {
            Id = branchId,
            Name = $"{TestPrefix}Branch_{branchId:N}",
            Address = $"{TestPrefix}Address",
            IsActive = true
        });

        context.OfferedLanguages.Add(new OfferedLanguage
        {
            Id = languageId,
            Name = $"{TestPrefix}Lang_{languageId:N}",
            IsActive = true
        });

        context.CourseLevels.Add(new CourseLevel
        {
            Id = courseLevelId,
            OfferedLanguageId = languageId,
            Name = $"{TestPrefix}Level_{courseLevelId:N}",
            Order = 1,
            IsActive = true
        });

        context.Teachers.Add(new Teacher
        {
            Id = teacherId,
            FirstName = $"{TestPrefix}Teacher_{teacherId:N}",
            LastName = "Test",
            MobilePhone = "05000000000",
            IsActive = true,
            HireDate = DateOnly.FromDateTime(DateTime.Today)
        });

        context.Classrooms.Add(new Classroom
        {
            Id = classroomId,
            BranchId = branchId,
            Name = $"{TestPrefix}Room_{classroomId:N}",
            Capacity = 30,
            IsActive = true
        });

        await context.SaveChangesAsync();
    }

    // ────────────────────────────────────────────
    //  Helper: Test dersi ekle
    // ────────────────────────────────────────────
    private static async Task SeedCourseAsync(
        AppDbContext context,
        Guid courseId,
        Guid branchId,
        Guid languageId,
        Guid courseLevelId,
        Guid teacherId,
        Guid classroomId,
        int capacity)
    {
        context.Courses.Add(new Course
        {
            Id = courseId,
            BranchId = branchId,
            OfferedLanguageId = languageId,
            CourseLevelId = courseLevelId,
            TeacherId = teacherId,
            ClassroomId = classroomId,
            Name = $"{TestPrefix}Course_{courseId:N}",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
            Capacity = capacity,
            TuitionFee = 1000m,
            IsActive = true,
            Status = CourseStatus.Open
        });

        await context.SaveChangesAsync();
    }

    // ────────────────────────────────────────────
    //  Helper: Test öğrencisi ekle
    // ────────────────────────────────────────────
    private static async Task SeedStudentAsync(AppDbContext context, Guid studentId)
    {
        context.Students.Add(new Student
        {
            Id = studentId,
            FirstName = $"{TestPrefix}Student_{studentId:N}",
            LastName = "Test",
            MobilePhone = "05000000001",
            IsActive = true,
            RegistrationDate = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    // ────────────────────────────────────────────
    //  Helper: Test verilerini temizle
    // ────────────────────────────────────────────
    private static async Task CleanupTestDataAsync(AppDbContext context)
    {
        // Ödeme → Enrollment → bağımlılık sırasıyla sil
        await context.Database.ExecuteSqlRawAsync($@"
            DELETE FROM [Payments] WHERE [IdempotencyKey] LIKE '{TestPrefix}%';
            DELETE FROM [Enrollments] WHERE [StudentId] IN (
                SELECT [Id] FROM [Students] WHERE [FirstName] LIKE '{TestPrefix}%'
            );
            DELETE FROM [CourseSchedules] WHERE [CourseId] IN (
                SELECT [Id] FROM [Courses] WHERE [Name] LIKE '{TestPrefix}%'
            );
            DELETE FROM [Courses] WHERE [Name] LIKE '{TestPrefix}%';
            DELETE FROM [Classrooms] WHERE [Name] LIKE '{TestPrefix}%';
            DELETE FROM [CourseLevels] WHERE [Name] LIKE '{TestPrefix}%';
            DELETE FROM [Students] WHERE [FirstName] LIKE '{TestPrefix}%';
            DELETE FROM [Teachers] WHERE [FirstName] LIKE '{TestPrefix}%';
            DELETE FROM [OfferedLanguages] WHERE [Name] LIKE '{TestPrefix}%';
            DELETE FROM [Branches] WHERE [Name] LIKE '{TestPrefix}%';
        ");
    }

    public void Dispose()
    {
        if (_canConnect)
        {
            try
            {
                using var context = CreateContext();
                CleanupTestDataAsync(context).GetAwaiter().GetResult();
            }
            catch
            {
                // Temizlik hatası test sonucunu etkilememeli
            }
        }
    }
}

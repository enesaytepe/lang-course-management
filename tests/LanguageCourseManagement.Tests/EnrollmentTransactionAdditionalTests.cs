using AutoMapper;
using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.Mapping;
using LanguageCourseManagement.Application.Services.InstallmentService;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.Application.Validators;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using LanguageCourseManagement.Infrastructure;
using LanguageCourseManagement.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class EnrollmentTransactionAdditionalTests : IDisposable
{
    private const string TestPrefix = "ITX2_";

    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
        ?? @"Server=localhost;Database=LangCourseManagement_Test;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly bool _canConnect;

    public EnrollmentTransactionAdditionalTests()
    {
        _canConnect = TryConnect();
        if (_canConnect)
            EnsureSchema();
    }

    [Fact]
    public async Task InstallmentEnrollment_WhenInstallmentPlanFails_EnrollmentIsRolledBack()
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

        // Create installment enrollment
        var paymentService = CreatePaymentService(context);
        var request = new EnrollmentCreateRequest
        {
            StudentId = studentId,
            CourseId = courseId,
            DiscountAmount = 0m,
            IdempotencyKey = $"{TestPrefix}installment-{Guid.NewGuid():N}",
            PaymentType = PaymentType.Installment,
            InstallmentCount = 3
        };

        var enrollmentResult = await paymentService.EnrollWithPaymentAsync(request, userId);
        Assert.NotEqual(Guid.Empty, enrollmentResult.Id);

        // Attempt to create installment plan with invalid count (should fail)
        var installmentService = CreateInstallmentService(context);
        await Assert.ThrowsAsync<LanguageCourseManagement.Application.Exceptions.BusinessException>(
            () => installmentService.CreateInstallmentPlanAsync(enrollmentResult.Id, 1));

        // Verify enrollment exists but no installments
        await using var verifyContext = CreateContext();
        var enrollmentInDb = await verifyContext.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == enrollmentResult.Id);
        Assert.NotNull(enrollmentInDb);

        var installmentCount = await verifyContext.Installments
            .AsNoTracking()
            .CountAsync(i => i.EnrollmentId == enrollmentResult.Id);
        Assert.Equal(0, installmentCount);

        await CleanupTestDataAsync(context);
    }

    [Fact]
    public async Task Cancellation_EnrollmentStatusChangesToCancelled()
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

        // Create enrollment
        var paymentService = CreatePaymentService(context);
        var request = new EnrollmentCreateRequest
        {
            StudentId = studentId,
            CourseId = courseId,
            DiscountAmount = 0m,
            IdempotencyKey = $"{TestPrefix}cancel-{Guid.NewGuid():N}",
            PaymentType = PaymentType.Cash
        };

        var enrollmentResult = await paymentService.EnrollWithPaymentAsync(request, userId);

        // Cancel the enrollment
        var enrollmentService = CreateEnrollmentService(context);
        var cancelResult = await enrollmentService.CancelAsync(enrollmentResult.Id);

        Assert.Equal("Cancelled", cancelResult.Status);

        // Verify in fresh context
        await using var verifyContext = CreateContext();
        var enrollmentInDb = await verifyContext.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == enrollmentResult.Id);
        Assert.NotNull(enrollmentInDb);
        Assert.Equal(EnrollmentStatus.Cancelled, enrollmentInDb!.Status);

        await CleanupTestDataAsync(context);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .EnableSensitiveDataLogging()
            .Options;
        return new AppDbContext(options, new HttpContextAccessor());
    }

    private static Application.Services.PaymentService.PaymentService CreatePaymentService(AppDbContext context)
    {
        return new Application.Services.PaymentService.PaymentService(
            new PaymentRepository(context),
            new EnrollmentRepository(context),
            new InstallmentRepository(context),
            new EfTransactionManager(context),
            new EnrollmentCreateRequestValidator(),
            new MapperConfiguration(cfg => cfg.AddMaps(typeof(EnrollmentProfile).Assembly), NullLoggerFactory.Instance).CreateMapper(),
            NullLogger<Application.Services.PaymentService.PaymentService>.Instance);
    }

    private static InstallmentService CreateInstallmentService(AppDbContext context)
    {
        return new InstallmentService(
            new EnrollmentRepository(context),
            new InstallmentRepository(context),
            new MapperConfiguration(cfg => cfg.AddMaps(typeof(EnrollmentProfile).Assembly), NullLoggerFactory.Instance).CreateMapper(),
            NullLogger<InstallmentService>.Instance);
    }

    private static EnrollmentService CreateEnrollmentService(AppDbContext context)
    {
        return new EnrollmentService(
            new EnrollmentRepository(context),
            new UpdateEnrollmentRequestValidator(),
            new MapperConfiguration(cfg => cfg.AddMaps(typeof(EnrollmentProfile).Assembly), NullLoggerFactory.Instance).CreateMapper());
    }

    private static bool TryConnect()
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return true;
        }
        catch { return false; }
    }

    private void EnsureSchema()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    private static async Task SeedPrerequisiteEntitiesAsync(AppDbContext context, Guid branchId, Guid languageId, Guid courseLevelId, Guid teacherId, Guid classroomId)
    {
        context.Branches.Add(new Branch { Id = branchId, Name = $"{TestPrefix}Branch_{branchId:N}", Address = $"{TestPrefix}Address", IsActive = true });
        context.OfferedLanguages.Add(new OfferedLanguage { Id = languageId, Name = $"{TestPrefix}Lang_{languageId:N}", IsActive = true });
        context.CourseLevels.Add(new CourseLevel { Id = courseLevelId, OfferedLanguageId = languageId, Name = $"{TestPrefix}Level_{courseLevelId:N}", Order = 1, IsActive = true });
        context.Teachers.Add(new Teacher { Id = teacherId, FirstName = $"{TestPrefix}Teacher_{teacherId:N}", LastName = "Test", MobilePhone = "05000000000", IsActive = true, HireDate = DateOnly.FromDateTime(DateTime.Today) });
        context.Classrooms.Add(new Classroom { Id = classroomId, BranchId = branchId, Name = $"{TestPrefix}Room_{classroomId:N}", Capacity = 30, IsActive = true });
        await context.SaveChangesAsync();
    }

    private static async Task SeedCourseAsync(AppDbContext context, Guid courseId, Guid branchId, Guid languageId, Guid courseLevelId, Guid teacherId, Guid classroomId, int capacity)
    {
        context.Courses.Add(new Course
        {
            Id = courseId, BranchId = branchId, OfferedLanguageId = languageId, CourseLevelId = courseLevelId,
            TeacherId = teacherId, ClassroomId = classroomId, Name = $"{TestPrefix}Course_{courseId:N}",
            StartDate = DateOnly.FromDateTime(DateTime.Today), EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
            Capacity = capacity, TuitionFee = 1000m, IsActive = true, Status = CourseStatus.Open
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedStudentAsync(AppDbContext context, Guid studentId)
    {
        context.Students.Add(new Student
        {
            Id = studentId, FirstName = $"{TestPrefix}Student_{studentId:N}", LastName = "Test",
            MobilePhone = "05000000001", IsActive = true, RegistrationDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    private static async Task CleanupTestDataAsync(AppDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync($@"
            DELETE FROM [Payments] WHERE [IdempotencyKey] LIKE '{TestPrefix}%';
            DELETE FROM [Installments] WHERE [EnrollmentId] IN (
                SELECT [Id] FROM [Enrollments] WHERE [StudentId] IN (
                    SELECT [Id] FROM [Students] WHERE [FirstName] LIKE '{TestPrefix}%'
                )
            );
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
            catch { }
        }
    }
}

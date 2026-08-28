using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using LanguageCourseManagement.Application.Persistence;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LanguageCourseManagement.Tests;

public static class TestBase
{
    public static CancellationTokenSource CreateCts() => new();

    public static Mock<IEnrollmentRepository> CreateEnrollmentRepository() => new();
    public static Mock<IPaymentRepository> CreatePaymentRepository() => new();
    public static Mock<IInstallmentRepository> CreateInstallmentRepository() => new();
    public static Mock<ITransactionManager> CreateTransactionManager() => new();
    public static Mock<IMapper> CreateMapper() => new();
    public static Mock<IValidator<T>> CreateValidator<T>() where T : class
    {
        var mock = new Mock<IValidator<T>>();
        mock.Setup(x => x.ValidateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return mock;
    }

    public static NullLogger<T> CreateNullLogger<T>() => NullLogger<T>.Instance;

    public static Enrollment CreateEnrollment(
        Guid? id = null,
        Guid? studentId = null,
        Guid? courseId = null,
        decimal tuitionFee = 100m,
        decimal discountAmount = 0m,
        decimal? finalAmount = null,
        EnrollmentStatus status = EnrollmentStatus.Active,
        PaymentType paymentType = PaymentType.Cash,
        List<Payment>? payments = null,
        List<Installment>? installments = null)
    {
        var actualFinalAmount = finalAmount ?? (tuitionFee - discountAmount);
        return new Enrollment
        {
            Id = id ?? Guid.NewGuid(),
            StudentId = studentId ?? Guid.NewGuid(),
            CourseId = courseId ?? Guid.NewGuid(),
            TuitionFee = tuitionFee,
            DiscountAmount = discountAmount,
            FinalAmount = actualFinalAmount,
            EnrollmentDate = DateTime.UtcNow,
            RegisteredByUserId = Guid.NewGuid(),
            Status = status,
            PaymentType = paymentType,
            Payments = payments,
            Installments = installments
        };
    }

    public static Course CreateCourse(
        Guid? id = null,
        int capacity = 20,
        decimal tuitionFee = 100m,
        bool isActive = true,
        CourseStatus status = CourseStatus.Open,
        Guid? branchId = null)
    {
        return new Course
        {
            Id = id ?? Guid.NewGuid(),
            BranchId = branchId ?? Guid.NewGuid(),
            OfferedLanguageId = Guid.NewGuid(),
            CourseLevelId = Guid.NewGuid(),
            TeacherId = Guid.NewGuid(),
            ClassroomId = Guid.NewGuid(),
            Name = "English A1",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
            Capacity = capacity,
            TuitionFee = tuitionFee,
            IsActive = isActive,
            Status = status
        };
    }

    public static Student CreateStudent(Guid? id = null)
    {
        return new Student
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Student",
            MobilePhone = "05000000000",
            IsActive = true,
            RegistrationDate = DateTime.UtcNow
        };
    }

    public static Payment CreatePayment(
        Guid? id = null,
        Guid? enrollmentId = null,
        decimal amount = 100m,
        PaymentMethod method = PaymentMethod.Cash,
        PaymentStatus status = PaymentStatus.Settled,
        string? idempotencyKey = null)
    {
        return new Payment
        {
            Id = id ?? Guid.NewGuid(),
            EnrollmentId = enrollmentId ?? Guid.NewGuid(),
            Amount = amount,
            Method = method,
            Status = status,
            SettledAt = DateTimeOffset.UtcNow,
            CollectedByUserId = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey ?? Guid.NewGuid().ToString("N"),
            PaymentDate = DateTime.UtcNow
        };
    }

    public static Installment CreateInstallment(
        Guid? id = null,
        Guid? enrollmentId = null,
        int installmentNumber = 1,
        decimal amount = 100m,
        PaymentStatus status = PaymentStatus.Pending)
    {
        return new Installment
        {
            Id = id ?? Guid.NewGuid(),
            EnrollmentId = enrollmentId ?? Guid.NewGuid(),
            InstallmentNumber = installmentNumber,
            Amount = amount,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(installmentNumber)),
            Status = status,
            Description = $"{installmentNumber}. taksit"
        };
    }
}

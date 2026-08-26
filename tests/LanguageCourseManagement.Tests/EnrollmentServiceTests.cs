using AutoMapper;
using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class EnrollmentServiceTests
{
    private readonly Mock<IEnrollmentRepository> enrollmentRepository = new();
    private readonly Mock<IPaymentRepository> paymentRepository = new();
    private readonly Mock<IValidator<EnrollmentCreateRequest>> createValidator = new();
    private readonly Mock<IValidator<UpdateEnrollmentRequest>> updateValidator = new();
    private readonly Mock<IMapper> mapper = new();

    [Fact]
    public async Task RegisterAndSettleAsync_rejects_duplicate_enrollment_and_does_not_stage_payment()
    {
        var request = Request();
        var existing = new Enrollment { Id = Guid.NewGuid(), StudentId = request.StudentId, CourseId = request.CourseId };
        enrollmentRepository.Setup(x => x.GetCourseForSettlementAsync(request.CourseId, It.IsAny<CancellationToken>())).ReturnsAsync(Course(capacity: 2));
        enrollmentRepository.Setup(x => x.GetActiveStudentAsync(request.StudentId, It.IsAny<CancellationToken>())).ReturnsAsync(Student(request.StudentId));
        enrollmentRepository.Setup(x => x.FindByStudentAndCourseAsync(request.StudentId, request.CourseId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        await Assert.ThrowsAsync<BusinessException>(() => CreateService().RegisterAndSettleAsync(request, Guid.NewGuid()));

        enrollmentRepository.Verify(x => x.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
        paymentRepository.Verify(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAndSettleAsync_rejects_full_capacity_and_does_not_stage_enrollment()
    {
        var request = Request();
        var course = Course(capacity: 1);
        enrollmentRepository.Setup(x => x.GetCourseForSettlementAsync(request.CourseId, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        enrollmentRepository.Setup(x => x.GetActiveStudentAsync(request.StudentId, It.IsAny<CancellationToken>())).ReturnsAsync(Student(request.StudentId));
        enrollmentRepository.Setup(x => x.CountActiveByCourseIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course.Capacity);

        await Assert.ThrowsAsync<BusinessException>(() => CreateService().RegisterAndSettleAsync(request, Guid.NewGuid()));

        enrollmentRepository.Verify(x => x.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAndSettleAsync_stages_exactly_one_enrollment_and_cash_payment()
    {
        var request = Request();
        var course = Course(capacity: 2);
        enrollmentRepository.Setup(x => x.GetCourseForSettlementAsync(request.CourseId, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        enrollmentRepository.Setup(x => x.GetActiveStudentAsync(request.StudentId, It.IsAny<CancellationToken>())).ReturnsAsync(Student(request.StudentId));
        enrollmentRepository.Setup(x => x.CountActiveByCourseIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        mapper.Setup(x => x.Map<EnrollmentDetailResponse>(It.IsAny<Enrollment>()))
            .Returns((Enrollment e) => new EnrollmentDetailResponse
            {
                Id = e.Id,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                TuitionFee = e.TuitionFee,
                DiscountAmount = e.DiscountAmount,
                FinalAmount = e.FinalAmount,
                Status = e.Status.ToString(),
                IsSettled = e.Payments.Any(),
                PaymentId = e.Payments.FirstOrDefault()?.Id
            });

        var result = await CreateService().RegisterAndSettleAsync(request, Guid.NewGuid());

        Assert.Equal(course.TuitionFee - request.DiscountAmount, result.FinalAmount);
        Assert.True(result.IsSettled);
        enrollmentRepository.Verify(x => x.AddAsync(It.Is<Enrollment>(e => e.FinalAmount == result.FinalAmount && e.Status == EnrollmentStatus.Active), It.IsAny<CancellationToken>()), Times.Once);
        paymentRepository.Verify(x => x.AddAsync(It.Is<Payment>(p => p.Amount == result.FinalAmount && p.Method == PaymentMethod.Cash && p.Status == PaymentStatus.Settled && p.IdempotencyKey == request.IdempotencyKey), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAndSettleAsync_replays_matching_idempotency_key_without_creating_second_payment()
    {
        var request = Request();
        var enrollment = new Enrollment { Id = Guid.NewGuid(), StudentId = request.StudentId, CourseId = request.CourseId, TuitionFee = 100m, DiscountAmount = 10m, FinalAmount = 90m };
        var payment = new Payment { Id = Guid.NewGuid(), EnrollmentId = enrollment.Id, Amount = 90m, Method = PaymentMethod.Cash, Status = PaymentStatus.Settled, IdempotencyKey = request.IdempotencyKey, Enrollment = enrollment };
        paymentRepository.Setup(x => x.FindByIdempotencyKeyAsync(request.IdempotencyKey, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        mapper.Setup(x => x.Map<EnrollmentDetailResponse>(It.IsAny<Enrollment>()))
            .Returns((Enrollment e) => new EnrollmentDetailResponse
            {
                Id = e.Id,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                FinalAmount = e.FinalAmount,
                IsSettled = e.Payments.Any(),
                PaymentId = e.Payments.FirstOrDefault()?.Id
            });

        var result = await CreateService().RegisterAndSettleAsync(request, Guid.NewGuid());

        Assert.Equal(enrollment.Id, result.Id);
        paymentRepository.Verify(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAndSettleAsync_rejects_idempotency_key_conflict()
    {
        var request = Request();
        var enrollment = new Enrollment { Id = Guid.NewGuid(), StudentId = Guid.NewGuid(), CourseId = request.CourseId, TuitionFee = 100m, FinalAmount = 100m };
        paymentRepository.Setup(x => x.FindByIdempotencyKeyAsync(request.IdempotencyKey, It.IsAny<CancellationToken>())).ReturnsAsync(new Payment { Enrollment = enrollment, Amount = 100m, Method = PaymentMethod.Cash });

        await Assert.ThrowsAsync<BusinessException>(() => CreateService().RegisterAndSettleAsync(request, Guid.NewGuid()));
    }

    private EnrollmentService CreateService()
    {
        mapper.Setup(x => x.Map<Enrollment>(It.IsAny<EnrollmentCreateRequest>()))
            .Returns((EnrollmentCreateRequest request) => new Enrollment { StudentId = request.StudentId, CourseId = request.CourseId, DiscountAmount = request.DiscountAmount });

        createValidator.Setup(x => x.ValidateAsync(It.IsAny<EnrollmentCreateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        updateValidator.Setup(x => x.ValidateAsync(It.IsAny<UpdateEnrollmentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        return new EnrollmentService(
            enrollmentRepository.Object,
            paymentRepository.Object,
            createValidator.Object,
            updateValidator.Object,
            mapper.Object);
    }

    private static EnrollmentCreateRequest Request() => new() { StudentId = Guid.NewGuid(), CourseId = Guid.NewGuid(), DiscountAmount = 10m, IdempotencyKey = "enrollment-001", PaymentType = PaymentType.Cash };

    private static Course Course(int capacity) => new() { Id = Guid.NewGuid(), Name = "English A1", Capacity = capacity, TuitionFee = 100m, IsActive = true, Status = CourseStatus.Open };

    private static Student Student(Guid id) => new() { Id = id, FirstName = "Test", LastName = "Student", IsActive = true };
}

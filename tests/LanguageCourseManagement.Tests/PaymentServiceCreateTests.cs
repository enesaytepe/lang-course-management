using AutoMapper;
using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.DTOs.Payments;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.PaymentService;
using LanguageCourseManagement.Application.Persistence;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class PaymentServiceCreateTests
{
    private readonly Mock<IPaymentRepository> paymentRepository = new();
    private readonly Mock<IEnrollmentRepository> enrollmentRepository = new();
    private readonly Mock<IInstallmentRepository> installmentRepository = new();
    private readonly Mock<ITransactionManager> transactionManager = new();
    private readonly Mock<IValidator<EnrollmentCreateRequest>> createValidator = new();
    private readonly Mock<IMapper> mapper = new();

    // Test 1: Cash enrollment creates settled payment
    [Fact]
    public async Task CreateAsync_cash_enrollment_creates_settled_payment()
    {
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enrollment = TestBase.CreateEnrollment(
            id: enrollmentId, studentId: studentId, courseId: courseId,
            tuitionFee: 100m, discountAmount: 0m, finalAmount: 100m,
            paymentType: PaymentType.Cash,
            payments: new List<Payment>(),
            installments: new List<Installment>());
        enrollment.Student = TestBase.CreateStudent(studentId);
        enrollment.Course = TestBase.CreateCourse(courseId);

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<Enrollment>, System.Linq.IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // PaymentService.CreateAsync calls GetByEnrollmentIdAsync for cash duplicate check
        paymentRepository.Setup(x => x.GetByEnrollmentIdAsync(enrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var service = CreateService();
        var request = new CreatePaymentRequest { EnrollmentId = enrollmentId, Method = PaymentMethod.Cash };

        var result = await service.CreateAsync(request, Guid.NewGuid());

        Assert.NotNull(result);
        paymentRepository.Verify(x => x.AddAsync(It.Is<Payment>(p =>
            p.EnrollmentId == enrollmentId &&
            p.Amount == 100m &&
            p.Status == PaymentStatus.Settled), It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test 2: Cash enrollment rejects duplicate payment
    [Fact]
    public async Task CreateAsync_cash_enrollment_rejects_duplicate_payment()
    {
        var enrollmentId = Guid.NewGuid();
        var enrollment = TestBase.CreateEnrollment(
            id: enrollmentId,
            paymentType: PaymentType.Cash,
            payments: new List<Payment> { TestBase.CreatePayment(enrollmentId: enrollmentId) });
        enrollment.Student = TestBase.CreateStudent();
        enrollment.Course = TestBase.CreateCourse();

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<Enrollment>, System.Linq.IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // PaymentService.CreateAsync calls GetByEnrollmentIdAsync for cash duplicate check
        paymentRepository.Setup(x => x.GetByEnrollmentIdAsync(enrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestBase.CreatePayment(enrollmentId: enrollmentId));

        var service = CreateService();
        var request = new CreatePaymentRequest { EnrollmentId = enrollmentId, Method = PaymentMethod.Cash };

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request, Guid.NewGuid()));
    }

    // Test 3: Installment enrollment requires installmentId
    [Fact]
    public async Task CreateAsync_installment_enrollment_requires_installmentId()
    {
        var enrollmentId = Guid.NewGuid();
        var enrollment = TestBase.CreateEnrollment(
            id: enrollmentId,
            paymentType: PaymentType.Installment,
            payments: new List<Payment>(),
            installments: new List<Installment> { TestBase.CreateInstallment(enrollmentId: enrollmentId) });
        enrollment.Student = TestBase.CreateStudent();
        enrollment.Course = TestBase.CreateCourse();

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<Enrollment>, System.Linq.IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        var service = CreateService();
        var request = new CreatePaymentRequest { EnrollmentId = enrollmentId, Method = PaymentMethod.Cash, InstallmentId = null };

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request, Guid.NewGuid()));
    }

    // Test 4: Installment enrollment sets correct amount
    [Fact]
    public async Task CreateAsync_installment_enrollment_sets_correct_amount()
    {
        var enrollmentId = Guid.NewGuid();
        var installmentId = Guid.NewGuid();
        var enrollment = TestBase.CreateEnrollment(
            id: enrollmentId,
            paymentType: PaymentType.Installment,
            finalAmount: 300m,
            payments: new List<Payment>(),
            installments: new List<Installment>
            {
                TestBase.CreateInstallment(id: installmentId, enrollmentId: enrollmentId, installmentNumber: 1, amount: 100m, status: PaymentStatus.Pending)
            });
        enrollment.Student = TestBase.CreateStudent();
        enrollment.Course = TestBase.CreateCourse();

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<Enrollment>, System.Linq.IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        var service = CreateService();
        var request = new CreatePaymentRequest { EnrollmentId = enrollmentId, Method = PaymentMethod.Cash, InstallmentId = installmentId };

        var result = await service.CreateAsync(request, Guid.NewGuid());

        Assert.Equal(100m, result.Amount);
    }

    // Test 5: Installment enrollment rejects already settled installment
    [Fact]
    public async Task CreateAsync_installment_enrollment_rejects_already_settled_installment()
    {
        var enrollmentId = Guid.NewGuid();
        var installmentId = Guid.NewGuid();
        var enrollment = TestBase.CreateEnrollment(
            id: enrollmentId,
            paymentType: PaymentType.Installment,
            payments: new List<Payment>(),
            installments: new List<Installment>
            {
                TestBase.CreateInstallment(id: installmentId, enrollmentId: enrollmentId, status: PaymentStatus.Settled)
            });
        enrollment.Student = TestBase.CreateStudent();
        enrollment.Course = TestBase.CreateCourse();

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<Enrollment>, System.Linq.IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        var service = CreateService();
        var request = new CreatePaymentRequest { EnrollmentId = enrollmentId, Method = PaymentMethod.Cash, InstallmentId = installmentId };

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request, Guid.NewGuid()));
    }

    // Test 6: Rejects inactive enrollment
    [Fact]
    public async Task CreateAsync_rejects_inactive_enrollment()
    {
        var enrollment = TestBase.CreateEnrollment(status: EnrollmentStatus.Cancelled);
        enrollment.Student = TestBase.CreateStudent();
        enrollment.Course = TestBase.CreateCourse();

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<Enrollment>, System.Linq.IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        var service = CreateService();
        var request = new CreatePaymentRequest { EnrollmentId = enrollment.Id, Method = PaymentMethod.Cash };

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request, Guid.NewGuid()));
    }

    // Test 7: Rejects nonexistent enrollment
    [Fact]
    public async Task CreateAsync_rejects_nonexistent_enrollment()
    {
        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<Enrollment>, System.Linq.IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        var service = CreateService();
        var request = new CreatePaymentRequest { EnrollmentId = Guid.NewGuid(), Method = PaymentMethod.Cash };

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(request, Guid.NewGuid()));
    }

    // Test 8: Cash completes remaining balance
    [Fact]
    public async Task CreateAsync_cash_completes_remaining_balance()
    {
        var enrollmentId = Guid.NewGuid();
        var existingPayment = TestBase.CreatePayment(enrollmentId: enrollmentId, amount: 60m, status: PaymentStatus.Settled);
        var enrollment = TestBase.CreateEnrollment(
            id: enrollmentId,
            tuitionFee: 100m,
            finalAmount: 100m,
            paymentType: PaymentType.Cash,
            payments: new List<Payment> { existingPayment });
        enrollment.Student = TestBase.CreateStudent();
        enrollment.Course = TestBase.CreateCourse();

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<Enrollment>, System.Linq.IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // PaymentService.CreateAsync calls GetByEnrollmentIdAsync for cash duplicate check
        paymentRepository.Setup(x => x.GetByEnrollmentIdAsync(enrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var service = CreateService();
        var request = new CreatePaymentRequest { EnrollmentId = enrollmentId, Method = PaymentMethod.Cash };

        var result = await service.CreateAsync(request, Guid.NewGuid());

        Assert.Equal(40m, result.Amount);
    }

    // Test 9: Cash rejects already fully paid
    [Fact]
    public async Task CreateAsync_cash_rejects_already_fully_paid()
    {
        var enrollmentId = Guid.NewGuid();
        var existingPayment = TestBase.CreatePayment(enrollmentId: enrollmentId, amount: 100m, status: PaymentStatus.Settled);
        var enrollment = TestBase.CreateEnrollment(
            id: enrollmentId,
            tuitionFee: 100m,
            finalAmount: 100m,
            paymentType: PaymentType.Cash,
            payments: new List<Payment> { existingPayment });
        enrollment.Student = TestBase.CreateStudent();
        enrollment.Course = TestBase.CreateCourse();

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<Enrollment>, System.Linq.IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // PaymentService.CreateAsync calls GetByEnrollmentIdAsync for cash duplicate check
        paymentRepository.Setup(x => x.GetByEnrollmentIdAsync(enrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var service = CreateService();
        var request = new CreatePaymentRequest { EnrollmentId = enrollmentId, Method = PaymentMethod.Cash };

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request, Guid.NewGuid()));
    }

    private PaymentService CreateService()
    {
        return new PaymentService(
            paymentRepository.Object,
            enrollmentRepository.Object,
            installmentRepository.Object,
            transactionManager.Object,
            createValidator.Object,
            mapper.Object,
            NullLogger<PaymentService>.Instance);
    }
}

using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.InstallmentService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class InstallmentServiceTests
{
    private readonly Mock<IEnrollmentRepository> enrollmentRepository = new();
    private readonly Mock<IInstallmentRepository> installmentRepository = new();
    private readonly Mock<IMapper> mapper = new();

    // Test 1: Creates correct number of installments
    [Fact]
    public async Task CreateInstallmentPlanAsync_creates_correct_number_of_installments()
    {
        var enrollmentId = Guid.NewGuid();
        var enrollment = new Enrollment
        {
            Id = enrollmentId,
            FinalAmount = 300m,
            PaymentType = PaymentType.Installment,
            Status = EnrollmentStatus.Active,
            EnrollmentDate = DateTime.UtcNow,
            Installments = null
        };

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<IQueryable<Enrollment>, IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        enrollmentRepository.Setup(x => x.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment e, CancellationToken _) => e);

        mapper.Setup(x => x.Map<InstallmentResponse>(It.IsAny<Installment>()))
            .Returns((Installment i) => new InstallmentResponse { Id = i.Id, Amount = i.Amount, InstallmentNumber = i.InstallmentNumber });

        var service = CreateService();
        var result = await service.CreateInstallmentPlanAsync(enrollmentId, 3);

        Assert.Equal(3, result.Count);
        Assert.Equal(100m, result[0].Amount);
        Assert.Equal(100m, result[1].Amount);
        Assert.Equal(100m, result[2].Amount);
    }

    // Test 2: Last installment covers rounding difference
    [Fact]
    public async Task CreateInstallmentPlanAsync_last_installment_covers_rounding_difference()
    {
        var enrollmentId = Guid.NewGuid();
        var enrollment = new Enrollment
        {
            Id = enrollmentId,
            FinalAmount = 100m,
            PaymentType = PaymentType.Installment,
            Status = EnrollmentStatus.Active,
            EnrollmentDate = DateTime.UtcNow,
            Installments = null
        };

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<IQueryable<Enrollment>, IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        enrollmentRepository.Setup(x => x.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment e, CancellationToken _) => e);

        mapper.Setup(x => x.Map<InstallmentResponse>(It.IsAny<Installment>()))
            .Returns((Installment i) => new InstallmentResponse { Id = i.Id, Amount = i.Amount, InstallmentNumber = i.InstallmentNumber });

        var service = CreateService();
        var result = await service.CreateInstallmentPlanAsync(enrollmentId, 3);

        Assert.Equal(3, result.Count);
        Assert.Equal(33.33m, result[0].Amount);
        Assert.Equal(33.33m, result[1].Amount);
        Assert.Equal(33.34m, result[2].Amount);
    }

    // Test 3: Rejects non-installment enrollment
    [Fact]
    public async Task CreateInstallmentPlanAsync_rejects_non_installment_enrollment()
    {
        var enrollmentId = Guid.NewGuid();
        var enrollment = new Enrollment
        {
            Id = enrollmentId,
            PaymentType = PaymentType.Cash,
            Status = EnrollmentStatus.Active,
            Installments = null
        };

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<IQueryable<Enrollment>, IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateInstallmentPlanAsync(enrollmentId, 3));
    }

    // Test 4: Rejects existing plan
    [Fact]
    public async Task CreateInstallmentPlanAsync_rejects_existing_plan()
    {
        var enrollmentId = Guid.NewGuid();
        var enrollment = new Enrollment
        {
            Id = enrollmentId,
            PaymentType = PaymentType.Installment,
            Status = EnrollmentStatus.Active,
            Installments = new List<Installment> { new Installment { Id = Guid.NewGuid(), EnrollmentId = enrollmentId } }
        };

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<IQueryable<Enrollment>, IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateInstallmentPlanAsync(enrollmentId, 3));
    }

    // Test 5: Rejects count below 2
    [Fact]
    public async Task CreateInstallmentPlanAsync_rejects_count_below_2()
    {
        var enrollmentId = Guid.NewGuid();
        var enrollment = new Enrollment
        {
            Id = enrollmentId,
            PaymentType = PaymentType.Installment,
            Status = EnrollmentStatus.Active,
            Installments = null
        };

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<IQueryable<Enrollment>, IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateInstallmentPlanAsync(enrollmentId, 1));
    }

    // Test 6: Rejects count above 12
    [Fact]
    public async Task CreateInstallmentPlanAsync_rejects_count_above_12()
    {
        var enrollmentId = Guid.NewGuid();
        var enrollment = new Enrollment
        {
            Id = enrollmentId,
            PaymentType = PaymentType.Installment,
            Status = EnrollmentStatus.Active,
            Installments = null
        };

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<IQueryable<Enrollment>, IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateInstallmentPlanAsync(enrollmentId, 13));
    }

    // Test 7: Rejects nonexistent enrollment
    [Fact]
    public async Task CreateInstallmentPlanAsync_rejects_nonexistent_enrollment()
    {
        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<IQueryable<Enrollment>, IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        var service = CreateService();
        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateInstallmentPlanAsync(Guid.NewGuid(), 3));
    }

    private InstallmentService CreateService()
    {
        return new InstallmentService(
            enrollmentRepository.Object,
            installmentRepository.Object,
            mapper.Object,
            NullLogger<InstallmentService>.Instance);
    }
}

using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class EnrollmentServiceAdditionalTests
{
    private readonly Mock<IEnrollmentRepository> enrollmentRepository = new();
    private readonly Mock<IValidator<UpdateEnrollmentRequest>> updateValidator = new();
    private readonly Mock<IMapper> mapper = new();

    [Fact]
    public async Task CancelAsync_sets_status_to_cancelled()
    {
        var enrollmentId = Guid.NewGuid();
        var enrollment = new Enrollment
        {
            Id = enrollmentId,
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            TuitionFee = 100m,
            DiscountAmount = 0m,
            FinalAmount = 100m,
            Status = EnrollmentStatus.Active,
            PaymentType = PaymentType.Cash,
            Student = new Student { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Student", IsActive = true },
            Course = new Course { Id = Guid.NewGuid(), Name = "English A1", BranchId = Guid.NewGuid(), IsActive = true, Status = CourseStatus.Open },
            Payments = new List<Payment>()
        };

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<IQueryable<Enrollment>, IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        updateValidator.Setup(x => x.ValidateAsync(It.IsAny<UpdateEnrollmentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        enrollmentRepository.Setup(x => x.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment e, CancellationToken _) => e);

        mapper.Setup(x => x.Map<EnrollmentDetailResponse>(It.IsAny<Enrollment>()))
            .Returns(new EnrollmentDetailResponse { Id = enrollmentId, Status = "Cancelled" });

        var service = CreateService();
        var result = await service.CancelAsync(enrollmentId);

        Assert.Equal("Cancelled", result.Status);
        enrollmentRepository.Verify(x => x.UpdateAsync(It.Is<Enrollment>(e =>
            e.Id == enrollmentId && e.Status == EnrollmentStatus.Cancelled), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_rejects_already_cancelled_enrollment()
    {
        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            Status = EnrollmentStatus.Cancelled,
            Student = new Student { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Student", IsActive = true },
            Course = new Course { Id = Guid.NewGuid(), Name = "English A1", IsActive = true, Status = CourseStatus.Open },
            Payments = new List<Payment>()
        };

        enrollmentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<Func<IQueryable<Enrollment>, IQueryable<Enrollment>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        updateValidator.Setup(x => x.ValidateAsync(It.IsAny<UpdateEnrollmentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() => service.CancelAsync(enrollment.Id));
    }

    [Fact]
    public async Task CheckEligibilityAsync_rejects_inactive_course()
    {
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        enrollmentRepository.Setup(x => x.GetActiveStudentAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = studentId, FirstName = "Test", LastName = "Student", IsActive = true });

        enrollmentRepository.Setup(x => x.GetCourseEligibilityInfoAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CourseEligibilityInfo { Id = courseId, Name = "English A1", Capacity = 20, IsActive = false, Status = CourseStatus.Open });

        var service = CreateService();
        var result = await service.CheckEligibilityAsync(studentId, courseId);

        Assert.False(result.IsEligible);
        Assert.Contains("kullanıma uygun değil", result.WarningMessage);
    }

    [Fact]
    public async Task CheckEligibilityAsync_rejects_non_open_course_status()
    {
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        enrollmentRepository.Setup(x => x.GetActiveStudentAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = studentId, FirstName = "Test", LastName = "Student", IsActive = true });

        enrollmentRepository.Setup(x => x.GetCourseEligibilityInfoAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CourseEligibilityInfo { Id = courseId, Name = "English A1", Capacity = 20, IsActive = true, Status = CourseStatus.Completed });

        var service = CreateService();
        var result = await service.CheckEligibilityAsync(studentId, courseId);

        Assert.False(result.IsEligible);
        Assert.Contains("kullanıma uygun değil", result.WarningMessage);
    }

    [Fact]
    public async Task CheckEligibilityAsync_rejects_nonexistent_student()
    {
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        enrollmentRepository.Setup(x => x.GetActiveStudentAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        var service = CreateService();
        var result = await service.CheckEligibilityAsync(studentId, courseId);

        Assert.False(result.IsEligible);
        Assert.Contains("Aktif öğrenci bulunamadı", result.WarningMessage);
    }

    [Fact]
    public async Task CheckEligibilityAsync_rejects_duplicate_enrollment()
    {
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var existingEnrollmentId = Guid.NewGuid();

        enrollmentRepository.Setup(x => x.GetActiveStudentAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = studentId, FirstName = "Test", LastName = "Student", IsActive = true });

        enrollmentRepository.Setup(x => x.GetCourseEligibilityInfoAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CourseEligibilityInfo { Id = courseId, Name = "English A1", Capacity = 20, IsActive = true, Status = CourseStatus.Open });

        enrollmentRepository.Setup(x => x.FindByStudentAndCourseAsync(studentId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Enrollment { Id = existingEnrollmentId, StudentId = studentId, CourseId = courseId, Status = EnrollmentStatus.Active });

        var service = CreateService();
        var result = await service.CheckEligibilityAsync(studentId, courseId);

        Assert.False(result.IsEligible);
        Assert.Contains("zaten kayıtlı", result.WarningMessage);
        Assert.Equal(existingEnrollmentId, result.ExistingEnrollmentId);
    }

    private EnrollmentService CreateService()
    {
        updateValidator.Setup(x => x.ValidateAsync(It.IsAny<UpdateEnrollmentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        mapper.Setup(x => x.Map<EnrollmentDetailResponse>(It.IsAny<Enrollment>()))
            .Returns((Enrollment e) => new EnrollmentDetailResponse
            {
                Id = e.Id,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                TuitionFee = e.TuitionFee,
                DiscountAmount = e.DiscountAmount,
                FinalAmount = e.FinalAmount,
                Status = e.Status.ToString()
            });

        return new EnrollmentService(
            enrollmentRepository.Object,
            updateValidator.Object,
            mapper.Object);
    }
}

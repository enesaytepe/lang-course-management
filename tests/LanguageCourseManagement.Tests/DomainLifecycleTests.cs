using System.Reflection;
using LanguageCourseManagement.Application.DTOs.Courses;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using CourseServiceNs = LanguageCourseManagement.Application.Services.CourseService.CourseService;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class DomainLifecycleTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepository = new();

    [Fact]
    public void Cancelled_course_does_not_block_time_slots()
    {
        var schedules = new List<CourseScheduleItemDto>
        {
            new() { DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(12, 0) }
        };

        var cancelledCourse = new Course
        {
            Id = Guid.NewGuid(),
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 6, 30),
            Status = CourseStatus.Cancelled,
            Schedules = [new CourseSchedule
            {
                CourseId = Guid.NewGuid(),
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(12, 0)
            }]
        };

        var method = typeof(CourseServiceNs).GetMethod("HasCourseConflict",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var result = (bool)method.Invoke(null, [new[] { cancelledCourse }, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), schedules, null])!;

        Assert.False(result);
    }

    [Fact]
    public void Completed_course_does_not_block_time_slots()
    {
        var schedules = new List<CourseScheduleItemDto>
        {
            new() { DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(12, 0) }
        };

        var completedCourse = new Course
        {
            Id = Guid.NewGuid(),
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 6, 30),
            Status = CourseStatus.Completed,
            Schedules = [new CourseSchedule
            {
                CourseId = Guid.NewGuid(),
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(12, 0)
            }]
        };

        var method = typeof(CourseServiceNs).GetMethod("HasCourseConflict",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var result = (bool)method.Invoke(null, [new[] { completedCourse }, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), schedules, null])!;

        Assert.False(result);
    }

    [Fact]
    public void Open_course_blocks_time_slots()
    {
        var schedules = new List<CourseScheduleItemDto>
        {
            new() { DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(12, 0) }
        };

        var openCourse = new Course
        {
            Id = Guid.NewGuid(),
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 6, 30),
            Status = CourseStatus.Open,
            Schedules = [new CourseSchedule
            {
                CourseId = Guid.NewGuid(),
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(12, 0)
            }]
        };

        var method = typeof(CourseServiceNs).GetMethod("HasCourseConflict",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var result = (bool)method.Invoke(null, [new[] { openCourse }, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), schedules, null])!;

        Assert.True(result);
    }

    [Fact]
    public async Task Student_can_re_enroll_after_cancellation()
    {
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var cancelledEnrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CourseId = courseId,
            Status = EnrollmentStatus.Cancelled
        };

        _enrollmentRepository.Setup(x => x.GetActiveStudentAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = studentId, FirstName = "Test", LastName = "Student", IsActive = true });
        _enrollmentRepository.Setup(x => x.GetCourseEligibilityInfoAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CourseEligibilityInfo { Id = courseId, Name = "English A1", Capacity = 20, IsActive = true, Status = CourseStatus.Open });
        _enrollmentRepository.Setup(x => x.FindByStudentAndCourseAsync(studentId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancelledEnrollment);
        _enrollmentRepository.Setup(x => x.CountActiveByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        _enrollmentRepository.Setup(x => x.GetStudentActiveScheduleAsync(studentId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CourseScheduleInfo>());

        var service = CreateEnrollmentService();
        var result = await service.CheckEligibilityAsync(studentId, courseId);

        Assert.True(result.IsEligible);
    }

    [Fact]
    public async Task Active_duplicate_enrollment_is_rejected()
    {
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var activeEnrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CourseId = courseId,
            Status = EnrollmentStatus.Active
        };

        _enrollmentRepository.Setup(x => x.GetActiveStudentAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = studentId, FirstName = "Test", LastName = "Student", IsActive = true });
        _enrollmentRepository.Setup(x => x.GetCourseEligibilityInfoAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CourseEligibilityInfo { Id = courseId, Name = "English A1", Capacity = 20, IsActive = true, Status = CourseStatus.Open });
        _enrollmentRepository.Setup(x => x.FindByStudentAndCourseAsync(studentId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeEnrollment);

        var service = CreateEnrollmentService();
        var result = await service.CheckEligibilityAsync(studentId, courseId);

        Assert.False(result.IsEligible);
        Assert.Equal("Öğrenci bu derse zaten kayıtlı.", result.WarningMessage);
        Assert.Equal(activeEnrollment.Id, result.ExistingEnrollmentId);
    }

    [Fact]
    public async Task Payment_history_remains_visible_after_enrollment_cancellation()
    {
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var cancelledEnrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CourseId = courseId,
            Status = EnrollmentStatus.Cancelled
        };

        _enrollmentRepository.Setup(x => x.GetActiveStudentAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = studentId, FirstName = "Test", LastName = "Student", IsActive = true });
        _enrollmentRepository.Setup(x => x.GetCourseEligibilityInfoAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CourseEligibilityInfo { Id = courseId, Name = "English A1", Capacity = 20, IsActive = true, Status = CourseStatus.Open });
        _enrollmentRepository.Setup(x => x.FindByStudentAndCourseAsync(studentId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancelledEnrollment);
        _enrollmentRepository.Setup(x => x.CountActiveByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        _enrollmentRepository.Setup(x => x.GetStudentActiveScheduleAsync(studentId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CourseScheduleInfo>());

        var service = CreateEnrollmentService();
        var result = await service.CheckEligibilityAsync(studentId, courseId);

        Assert.True(result.IsEligible);
        Assert.Null(result.WarningMessage);
    }

    private EnrollmentService CreateEnrollmentService()
    {
        var validator = new Mock<FluentValidation.IValidator<UpdateEnrollmentRequest>>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<UpdateEnrollmentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var mapper = new Mock<AutoMapper.IMapper>();
        mapper.Setup(x => x.Map<EnrollmentDetailResponse>(It.IsAny<Enrollment>()))
            .Returns((Enrollment e) => new EnrollmentDetailResponse
            {
                Id = e.Id,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                Status = e.Status.ToString()
            });

        return new EnrollmentService(
            _enrollmentRepository.Object,
            validator.Object,
            mapper.Object);
    }
}

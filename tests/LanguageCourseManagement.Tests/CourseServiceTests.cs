using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using LanguageCourseManagement.Application.DTOs.Courses;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.CourseService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class CourseServiceTests
{
    private readonly Mock<ICourseRepository> courseRepository = new();
    private readonly Mock<IBranchRepository> branchRepository = new();
    private readonly Mock<IOfferedLanguageRepository> languageRepository = new();
    private readonly Mock<ICourseLevelRepository> levelRepository = new();
    private readonly Mock<ITeacherRepository> teacherRepository = new();
    private readonly Mock<IClassroomRepository> classroomRepository = new();
    private readonly Mock<IEnrollmentRepository> enrollmentRepository = new();
    private readonly Mock<IMapper> mapper = new();
    private readonly Mock<IValidator<CreateCourseRequest>> createValidator = new();
    private readonly Mock<IValidator<UpdateCourseRequest>> updateValidator = new();

    /// <summary>
    /// Sets up all repository mocks so that CreateAsync passes all Ensure checks
    /// (name uniqueness, branch, language, level, teacher, classroom).
    /// Use this to reach schedule validation or later logic.
    /// </summary>
    private void SetupHappyPath(
        Guid branchId, Guid languageId, Guid levelId, Guid teacherId, Guid classroomId,
        bool branchActive = true, bool languageActive = true, bool levelActive = true,
        bool teacherActive = true, bool classroomActive = true)
    {
        // EnsureCourseNameUniqueAsync — name does not exist
        courseRepository.Setup(x => x.AnyAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Course, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // EnsureBranchExistsAsync
        var branch = new Branch { Id = branchId, IsActive = branchActive };
        branchRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>(),
            It.IsAny<Func<IQueryable<Branch>, IQueryable<Branch>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        // EnsureLanguageExistsAsync
        var language = new OfferedLanguage { Id = languageId, IsActive = languageActive };
        languageRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<OfferedLanguage, bool>>>(),
            It.IsAny<Func<IQueryable<OfferedLanguage>, IQueryable<OfferedLanguage>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(language);

        // EnsureLevelExistsAsync
        var level = new CourseLevel { Id = levelId, OfferedLanguageId = languageId, IsActive = levelActive };
        levelRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CourseLevel, bool>>>(),
            It.IsAny<Func<IQueryable<CourseLevel>, IQueryable<CourseLevel>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(level);

        // EnsureTeacherExistsAsync + ValidateScheduleRulesAsync re-fetch
        var teacher = new Teacher
        {
            Id = teacherId,
            IsActive = teacherActive,
            TeacherLanguages = new List<TeacherLanguage> { new TeacherLanguage { TeacherId = teacherId, OfferedLanguageId = languageId } },
            TeacherBranches = new List<TeacherBranch> { new TeacherBranch { TeacherId = teacherId, BranchId = branchId } },
            Availabilities = new List<TeacherAvailability>(),
            Courses = new List<Course>()
        };
        teacherRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Teacher, bool>>>(),
            It.IsAny<Func<IQueryable<Teacher>, IQueryable<Teacher>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        // EnsureClassroomExistsAsync + ValidateScheduleRulesAsync re-fetch
        var classroom = new Classroom
        {
            Id = classroomId,
            BranchId = branchId,
            Capacity = 30,
            IsActive = classroomActive,
            Courses = new List<Course>()
        };
        classroomRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Classroom, bool>>>(),
            It.IsAny<Func<IQueryable<Classroom>, IQueryable<Classroom>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(classroom);
    }

    private CreateCourseRequest CreateValidRequest(
        Guid? branchId = null, Guid? languageId = null, Guid? levelId = null,
        Guid? teacherId = null, Guid? classroomId = null)
    {
        var bId = branchId ?? Guid.NewGuid();
        var lId = languageId ?? Guid.NewGuid();
        return new CreateCourseRequest
        {
            Name = "English A1",
            BranchId = bId,
            OfferedLanguageId = lId,
            CourseLevelId = levelId ?? Guid.NewGuid(),
            TeacherId = teacherId ?? Guid.NewGuid(),
            ClassroomId = classroomId ?? Guid.NewGuid(),
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
            Capacity = 10,
            TuitionFee = 500m,
            Status = CourseStatus.Draft,
            Schedules = new List<CourseScheduleItemDto>
            {
                new CourseScheduleItemDto { DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(12, 0) }
            }
        };
    }

    // Test 1: Rejects empty schedules
    [Fact]
    public async Task CreateAsync_rejects_empty_schedules()
    {
        var request = CreateValidRequest();
        request.Schedules = new List<CourseScheduleItemDto>();

        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Must pass all Ensure checks before reaching schedule validation
        SetupHappyPath(request.BranchId, request.OfferedLanguageId, request.CourseLevelId, request.TeacherId, request.ClassroomId);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    // Test 2: Rejects start time >= end time
    [Fact]
    public async Task CreateAsync_rejects_start_time_after_end_time()
    {
        var request = CreateValidRequest();
        request.Schedules = new List<CourseScheduleItemDto>
        {
            new CourseScheduleItemDto { DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(10, 0) }
        };

        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        SetupHappyPath(request.BranchId, request.OfferedLanguageId, request.CourseLevelId, request.TeacherId, request.ClassroomId);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    // Test 3: Rejects duplicate day of week
    [Fact]
    public async Task CreateAsync_rejects_duplicate_day_of_week()
    {
        var request = CreateValidRequest();
        request.Schedules = new List<CourseScheduleItemDto>
        {
            new CourseScheduleItemDto { DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(12, 0) },
            new CourseScheduleItemDto { DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(16, 0) }
        };

        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        SetupHappyPath(request.BranchId, request.OfferedLanguageId, request.CourseLevelId, request.TeacherId, request.ClassroomId);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    // Test 4: Rejects inactive branch
    [Fact]
    public async Task CreateAsync_rejects_inactive_branch()
    {
        var request = CreateValidRequest();
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        SetupHappyPath(request.BranchId, request.OfferedLanguageId, request.CourseLevelId, request.TeacherId, request.ClassroomId,
            branchActive: false);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    // Test 5: Rejects inactive language
    [Fact]
    public async Task CreateAsync_rejects_inactive_language()
    {
        var request = CreateValidRequest();
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        SetupHappyPath(request.BranchId, request.OfferedLanguageId, request.CourseLevelId, request.TeacherId, request.ClassroomId,
            languageActive: false);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    // Test 6: Rejects level mismatch
    [Fact]
    public async Task CreateAsync_rejects_level_mismatch()
    {
        var request = CreateValidRequest();
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        SetupHappyPath(request.BranchId, request.OfferedLanguageId, request.CourseLevelId, request.TeacherId, request.ClassroomId);

        // Override level to have a different OfferedLanguageId than the request
        var level = new CourseLevel { Id = request.CourseLevelId, OfferedLanguageId = Guid.NewGuid(), IsActive = true };
        levelRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CourseLevel, bool>>>(),
            It.IsAny<Func<IQueryable<CourseLevel>, IQueryable<CourseLevel>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(level);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    // Test 7: Rejects inactive teacher
    [Fact]
    public async Task CreateAsync_rejects_inactive_teacher()
    {
        var request = CreateValidRequest();
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        SetupHappyPath(request.BranchId, request.OfferedLanguageId, request.CourseLevelId, request.TeacherId, request.ClassroomId,
            teacherActive: false);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    // Test 8: Rejects teacher language mismatch
    [Fact]
    public async Task CreateAsync_rejects_teacher_language_mismatch()
    {
        var request = CreateValidRequest();
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var branchId = request.BranchId;
        var languageId = request.OfferedLanguageId;
        var teacherId = request.TeacherId;
        SetupHappyPath(branchId, languageId, request.CourseLevelId, teacherId, request.ClassroomId);

        // Override teacher to not teach the requested language
        var teacher = new Teacher
        {
            Id = teacherId,
            IsActive = true,
            TeacherLanguages = new List<TeacherLanguage>(),
            TeacherBranches = new List<TeacherBranch> { new TeacherBranch { TeacherId = teacherId, BranchId = branchId } },
            Availabilities = new List<TeacherAvailability>(),
            Courses = new List<Course>()
        };
        teacherRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Teacher, bool>>>(),
            It.IsAny<Func<IQueryable<Teacher>, IQueryable<Teacher>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    // Test 9: Rejects teacher branch mismatch
    [Fact]
    public async Task CreateAsync_rejects_teacher_branch_mismatch()
    {
        var request = CreateValidRequest();
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var languageId = request.OfferedLanguageId;
        var teacherId = request.TeacherId;
        SetupHappyPath(request.BranchId, languageId, request.CourseLevelId, teacherId, request.ClassroomId);

        // Override teacher to not teach in the requested branch
        var teacher = new Teacher
        {
            Id = teacherId,
            IsActive = true,
            TeacherLanguages = new List<TeacherLanguage> { new TeacherLanguage { TeacherId = teacherId, OfferedLanguageId = languageId } },
            TeacherBranches = new List<TeacherBranch>(),
            Availabilities = new List<TeacherAvailability>(),
            Courses = new List<Course>()
        };
        teacherRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Teacher, bool>>>(),
            It.IsAny<Func<IQueryable<Teacher>, IQueryable<Teacher>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    // Test 10: Rejects inactive classroom
    [Fact]
    public async Task CreateAsync_rejects_inactive_classroom()
    {
        var request = CreateValidRequest();
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        SetupHappyPath(request.BranchId, request.OfferedLanguageId, request.CourseLevelId, request.TeacherId, request.ClassroomId,
            classroomActive: false);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    // Test 11: Rejects classroom branch mismatch
    [Fact]
    public async Task CreateAsync_rejects_classroom_branch_mismatch()
    {
        var request = CreateValidRequest();
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var branchId = request.BranchId;
        SetupHappyPath(branchId, request.OfferedLanguageId, request.CourseLevelId, request.TeacherId, request.ClassroomId);

        // Override classroom to belong to a different branch
        var classroom = new Classroom
        {
            Id = request.ClassroomId,
            BranchId = Guid.NewGuid(),
            Capacity = 30,
            IsActive = true,
            Courses = new List<Course>()
        };
        classroomRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Classroom, bool>>>(),
            It.IsAny<Func<IQueryable<Classroom>, IQueryable<Classroom>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(classroom);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    // Test 12: DeleteAsync rejects course with active enrollments
    [Fact]
    public async Task DeleteAsync_rejects_course_with_enrollments()
    {
        var courseId = Guid.NewGuid();
        var course = new Course { Id = courseId, Name = "Test", Schedules = null };

        courseRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Course, bool>>>(),
            It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // No schedules
        courseRepository.Setup(x => x.AnyAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Course, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Has active enrollments
        enrollmentRepository.Setup(x => x.AnyAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.DeleteAsync(courseId));
    }

    // Test 13: DeleteAsync deletes course and its schedules
    [Fact]
    public async Task DeleteAsync_deletes_course_and_schedules()
    {
        var courseId = Guid.NewGuid();
        var course = new Course { Id = courseId, Name = "Test", Schedules = null };

        courseRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Course, bool>>>(),
            It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // No active enrollments
        enrollmentRepository.Setup(x => x.AnyAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Enrollment, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Mock DeleteSchedulesByCourseIdAsync
        courseRepository.Setup(x => x.DeleteSchedulesByCourseIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Mock DeleteAsync
        courseRepository.Setup(x => x.DeleteAsync(
            It.IsAny<Course>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // Mock mapper: Course -> CourseResponse
        var courseResponse = new CourseResponse { Id = courseId, Name = "Test" };
        mapper.Setup(x => x.Map<CourseResponse>(It.IsAny<Course>()))
            .Returns(courseResponse);

        var service = CreateService();
        var response = await service.DeleteAsync(courseId);

        Assert.NotNull(response);
        Assert.Equal(courseId, response.Id);

        // Verify schedules were deleted
        courseRepository.Verify(x => x.DeleteSchedulesByCourseIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);

        // Verify course was deleted
        courseRepository.Verify(x => x.DeleteAsync(course, It.IsAny<CancellationToken>()), Times.Once);
    }

    private CourseService CreateService()
    {
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // _mapper.Map<Course>(request) is called before ValidateScheduleRulesAsync in CreateAsync
        mapper.Setup(x => x.Map<Course>(It.IsAny<CreateCourseRequest>()))
            .Returns(new Course());

        return new CourseService(
            courseRepository.Object,
            branchRepository.Object,
            languageRepository.Object,
            levelRepository.Object,
            teacherRepository.Object,
            classroomRepository.Object,
            enrollmentRepository.Object,
            mapper.Object,
            NullLogger<CourseService>.Instance,
            createValidator.Object,
            updateValidator.Object);
    }
}

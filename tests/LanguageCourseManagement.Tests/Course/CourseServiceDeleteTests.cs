using System.Collections;
using System.Linq.Expressions;
using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Courses;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Mapping;
using LanguageCourseManagement.Application.Services.CourseService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests.CourseDelete;

public sealed class CourseServiceDeleteTests
{
    [Fact]
    public async Task Delete_succeeds_when_course_has_schedules_but_no_enrollments()
    {
        var courseId = Guid.NewGuid();
        var course = CreateCourseWithSchedules(courseId, scheduleCount: 2);
        var (service, courseRepository, enrollmentRepository) = CreateService(course);

        var response = await service.DeleteAsync(courseId);

        Assert.NotNull(response);
        Assert.Equal(courseId, response.Id);
        courseRepository.Verify(
            repo => repo.DeleteSchedulesByCourseIdAsync(courseId, It.IsAny<CancellationToken>()),
            Times.Once);
        courseRepository.Verify(
            repo => repo.DeleteAsync(course, It.IsAny<CancellationToken>()),
            Times.Once);
        enrollmentRepository.Verify(
            repo => repo.AnyAsync(
                It.IsAny<Expression<Func<Enrollment, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_succeeds_for_newly_created_course_with_single_schedule()
    {
        var courseId = Guid.NewGuid();
        var course = CreateCourseWithSchedules(courseId, scheduleCount: 1);
        var (service, courseRepository, enrollmentRepository) = CreateService(course);

        var response = await service.DeleteAsync(courseId);

        Assert.NotNull(response);
        Assert.Equal(courseId, response.Id);
        courseRepository.Verify(
            repo => repo.DeleteSchedulesByCourseIdAsync(courseId, It.IsAny<CancellationToken>()),
            Times.Once);
        courseRepository.Verify(
            repo => repo.DeleteAsync(course, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_rejects_when_active_enrollments_exist()
    {
        var courseId = Guid.NewGuid();
        var course = CreateCourseWithSchedules(courseId, scheduleCount: 2);
        var (service, courseRepository, enrollmentRepository) = CreateService(course);

        enrollmentRepository
            .Setup(repo => repo.AnyAsync(
                It.IsAny<Expression<Func<Enrollment, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.DeleteAsync(courseId));

        Assert.Contains("aktif öğrenci kaydı", exception.Message);
        courseRepository.Verify(
            repo => repo.DeleteSchedulesByCourseIdAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        courseRepository.Verify(
            repo => repo.DeleteAsync(
                It.IsAny<Course>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_removes_all_schedules_when_succeeding()
    {
        var courseId = Guid.NewGuid();
        var course = CreateCourseWithSchedules(courseId, scheduleCount: 3);
        var (service, courseRepository, _) = CreateService(course);

        await service.DeleteAsync(courseId);

        courseRepository.Verify(
            repo => repo.DeleteSchedulesByCourseIdAsync(courseId, It.IsAny<CancellationToken>()),
            Times.Once);
        courseRepository.Verify(
            repo => repo.DeleteAsync(course, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_preserves_course_and_schedules_when_enrollment_blocks()
    {
        var courseId = Guid.NewGuid();
        var course = CreateCourseWithSchedules(courseId, scheduleCount: 2);
        var (service, courseRepository, enrollmentRepository) = CreateService(course);

        enrollmentRepository
            .Setup(repo => repo.AnyAsync(
                It.IsAny<Expression<Func<Enrollment, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessException>(
            () => service.DeleteAsync(courseId));

        courseRepository.Verify(
            repo => repo.DeleteSchedulesByCourseIdAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        courseRepository.Verify(
            repo => repo.DeleteAsync(
                It.IsAny<Course>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_does_not_block_on_completed_or_cancelled_enrollments()
    {
        var courseId = Guid.NewGuid();
        var course = CreateCourseWithSchedules(courseId, scheduleCount: 2);
        var (service, courseRepository, enrollmentRepository) = CreateService(course);

        // AnyAsync returns false => no active enrollments
        enrollmentRepository
            .Setup(repo => repo.AnyAsync(
                It.IsAny<Expression<Func<Enrollment, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var response = await service.DeleteAsync(courseId);

        Assert.NotNull(response);
        Assert.Equal(courseId, response.Id);
        courseRepository.Verify(
            repo => repo.DeleteSchedulesByCourseIdAsync(courseId, It.IsAny<CancellationToken>()),
            Times.Once);
        courseRepository.Verify(
            repo => repo.DeleteAsync(course, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ──────────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────────

    private static Course CreateCourseWithSchedules(Guid courseId, int scheduleCount)
    {
        var schedules = Enumerable.Range(0, scheduleCount)
            .Select(i => new CourseSchedule
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                DayOfWeek = (DayOfWeek)i,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(11, 0)
            })
            .ToList();

        return new Course
        {
            Id = courseId,
            Name = "Test Course",
            BranchId = Guid.NewGuid(),
            OfferedLanguageId = Guid.NewGuid(),
            CourseLevelId = Guid.NewGuid(),
            TeacherId = Guid.NewGuid(),
            ClassroomId = Guid.NewGuid(),
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 6, 30),
            Capacity = 20,
            TuitionFee = 5000m,
            IsActive = true,
            Status = CourseStatus.Open,
            Schedules = schedules,
            Branch = new Branch { Id = Guid.NewGuid(), Name = "Test Branch", IsActive = true },
            OfferedLanguage = new OfferedLanguage { Id = Guid.NewGuid(), Name = "English", IsActive = true },
            CourseLevel = new CourseLevel { Id = Guid.NewGuid(), Name = "A1", IsActive = true },
            Teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Teacher", IsActive = true },
            Classroom = new Classroom { Id = Guid.NewGuid(), Name = "Room 1", Capacity = 20, IsActive = true }
        };
    }

    private static (CourseService service, Mock<ICourseRepository> courseRepo, Mock<IEnrollmentRepository> enrollmentRepo) CreateService(
        Course course)
    {
        var courseRepository = new Mock<ICourseRepository>();
        courseRepository
            .Setup(repo => repo.GetAsync(
                It.IsAny<Expression<Func<Course, bool>>>(),
                It.IsAny<Func<IQueryable<Course>, IIncludableQueryable<Course, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        courseRepository
            .Setup(repo => repo.DeleteAsync(course, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        courseRepository
            .Setup(repo => repo.DeleteSchedulesByCourseIdAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var enrollmentRepository = new Mock<IEnrollmentRepository>();
        enrollmentRepository
            .Setup(repo => repo.AnyAsync(
                It.IsAny<Expression<Func<Enrollment, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var branchRepository = new Mock<IBranchRepository>();
        var offeredLanguageRepository = new Mock<IOfferedLanguageRepository>();
        var courseLevelRepository = new Mock<ICourseLevelRepository>();
        var teacherRepository = new Mock<ITeacherRepository>();
        var classroomRepository = new Mock<IClassroomRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CourseProfile>();
        }, NullLoggerFactory.Instance);
        var mapper = mapperConfig.CreateMapper();

        var service = new CourseService(
            courseRepository.Object,
            branchRepository.Object,
            offeredLanguageRepository.Object,
            courseLevelRepository.Object,
            teacherRepository.Object,
            classroomRepository.Object,
            enrollmentRepository.Object,
            mapper,
            NullLogger<CourseService>.Instance,
            new Mock<FluentValidation.IValidator<CreateCourseRequest>>().Object,
            new Mock<FluentValidation.IValidator<UpdateCourseRequest>>().Object);

        return (service, courseRepository, enrollmentRepository);
    }

    // ──────────────────────────────────────────────────────────
    //  AsyncQueryable helpers so FirstOrDefaultAsync works
    //  on in-memory IQueryable
    // ──────────────────────────────────────────────────────────

    private static IQueryable<T> ToAsyncQueryable<T>(IEnumerable<T> source)
    {
        var queryable = source.AsQueryable();
        var provider = new AsyncQueryProvider<T>(queryable.Provider);
        return new AsyncEnumerable<T>(queryable.Expression, provider);
    }

    private sealed class AsyncQueryProvider<T> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public AsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetGenericArguments().Length > 0
                ? expression.Type.GetGenericArguments()[0]
                : typeof(T);

            return (IQueryable)typeof(AsyncEnumerable<>)
                .MakeGenericType(elementType)
                .GetConstructor(new[] { typeof(Expression), typeof(IAsyncQueryProvider) })!
                .Invoke(new object[] { expression, this });
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new AsyncEnumerable<TElement>(expression, new AsyncQueryProvider<TElement>(_inner));

        public object? Execute(Expression expression) => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult);

            Type? valueType = null;
            bool isValueTask = false;
            bool isTask = false;

            if (resultType.IsGenericType)
            {
                var def = resultType.GetGenericTypeDefinition();
                if (def == typeof(ValueTask<>)) { isValueTask = true; valueType = resultType.GetGenericArguments()[0]; }
                else if (def == typeof(Task<>)) { isTask = true; valueType = resultType.GetGenericArguments()[0]; }
            }

            var lambda = Expression.Lambda<Func<object>>(
                Expression.Convert(expression, typeof(object)));
            var result = lambda.Compile()();

            if (valueType != null)
            {
                if (isTask)
                {
                    var fromResult = typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(valueType);
                    return (TResult)fromResult.Invoke(null, new[] { result })!;
                }
                if (isValueTask)
                {
                    var vt = Activator.CreateInstance(typeof(ValueTask<>).MakeGenericType(valueType), result)!;
                    return (TResult)vt;
                }
            }

            return (TResult)result!;
        }
    }

    private sealed class AsyncEnumerable<T> : IQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly Expression _expression;
        private readonly IAsyncQueryProvider _provider;

        public AsyncEnumerable(Expression expression, IAsyncQueryProvider provider)
        {
            _expression = expression;
            _provider = provider;
        }

        public Type ElementType => typeof(T);
        public Expression Expression => _expression;
        public IQueryProvider Provider => _provider;

        public IEnumerator<T> GetEnumerator() =>
            _provider.Execute<IEnumerable<T>>(_expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new AsyncEnumerator<T>(GetEnumerator());
    }

    private sealed class AsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public AsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public T Current => _inner.Current;
        public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    }
}

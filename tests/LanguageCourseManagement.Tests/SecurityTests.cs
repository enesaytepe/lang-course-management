using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.DTOs.Branches;
using LanguageCourseManagement.Application.DTOs.Students;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.BranchService;
using LanguageCourseManagement.Application.Services.ClassroomService;
using LanguageCourseManagement.Application.Services.CourseLevelService;
using LanguageCourseManagement.Application.Services.CourseService;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.Application.Services.FacilityService;
using LanguageCourseManagement.Application.Services.OfferedLanguageService;
using LanguageCourseManagement.Application.Services.PaymentService;
using LanguageCourseManagement.Application.Services.StudentService;
using LanguageCourseManagement.Application.Services.TeacherService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using LanguageCourseManagement.Infrastructure;
using LanguageCourseManagement.MVC.Controllers.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

/// <summary>
/// Regression tests for the "showDeleted" security vulnerability fix (plan 041).
///
/// The vulnerability: list endpoints and services exposed a "showDeleted" flag and repositories
/// exposed <c>QueryWithIgnoreFilters</c>, letting any caller bypass the soft-delete global query
/// filter. The fix removed all of those surfaces; the global <c>!IsDeleted</c> query filter on
/// <see cref="AppDbContext"/> is now the single source of truth for deleted-record visibility.
///
/// These tests lock that contract in three layers:
/// 1. Reflection-based contract tests: no service interface, no <see cref="IRepository{TEntity}"/>
///    and no API controller exposes a "showDeleted" parameter or <c>QueryWithIgnoreFilters</c>.
/// 2. Behavior tests (Moq): list services read through the default filtered repository query path
///    and return exactly what it yields; a missing/soft-deleted record surfaces as NotFound at the
///    service level (same pattern as <c>StudentServiceTests.DeleteAsync_rejects_nonexistent_entity</c>).
/// 3. EF model tests: every soft-deletable entity type in <see cref="AppDbContext"/> carries a
///    global query filter, and the core entities behind the fixed endpoints filter on their own
///    <c>IsDeleted</c> flag. These are model-building checks only — no database connection is made.
///
/// A real-database integration test for the soft-delete filter could not be added: the existing
/// <c>SqlServerEnrollmentFixture</c> is an explicit not-verified placeholder (no disposable SQL
/// Server database is configured), and faking it is prohibited by repo rules.
/// </summary>
public sealed class SecurityTests
{
    private const string ShowDeletedParameter = "showDeleted";

    // -----------------------------------------------------------------------------------------
    // 1. Contract tests — service interfaces must not expose a showDeleted parameter
    // -----------------------------------------------------------------------------------------

    public static IEnumerable<object[]> SoftDeleteListServiceInterfaces() => new[]
    {
        new object[] { typeof(IStudentService) },
        new object[] { typeof(IBranchService) },
        new object[] { typeof(ITeacherService) },
        new object[] { typeof(ICourseService) },
        new object[] { typeof(IClassroomService) },
        new object[] { typeof(ICourseLevelService) },
        new object[] { typeof(IOfferedLanguageService) },
        new object[] { typeof(IFacilityService) },
        new object[] { typeof(IEnrollmentService) },
        new object[] { typeof(IPaymentService) },
    };

    [Theory]
    [MemberData(nameof(SoftDeleteListServiceInterfaces))]
    public void ServiceInterface_GetListAsync_HasNoShowDeletedParameter(Type serviceInterface)
    {
        var getListMethods = serviceInterface.GetMethods()
            .Where(m => m.Name == nameof(IStudentService.GetListAsync))
            .ToList();

        Assert.NotEmpty(getListMethods);

        var offenders = getListMethods
            .SelectMany(m => m.GetParameters())
            .Where(p => string.Equals(p.Name, ShowDeletedParameter, StringComparison.OrdinalIgnoreCase))
            .Select(p => $"{p.ParameterType.Name} {p.Name}")
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"{serviceInterface.Name}.GetListAsync must not expose a '{ShowDeletedParameter}' parameter " +
            $"(soft-delete bypass surface); found: {string.Join(", ", offenders)}");
    }

    [Theory]
    [MemberData(nameof(SoftDeleteListServiceInterfaces))]
    public void ServiceInterface_NoMethodAtAll_ExposesShowDeletedParameter(Type serviceInterface)
    {
        var offenders = serviceInterface.GetMethods()
            .SelectMany(m => m.GetParameters().Select(p => new { Method = m.Name, Param = p }))
            .Where(x => string.Equals(x.Param.Name, ShowDeletedParameter, StringComparison.OrdinalIgnoreCase))
            .Select(x => $"{x.Method}({x.Param.ParameterType.Name} {x.Param.Name})")
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"{serviceInterface.Name} must not expose '{ShowDeletedParameter}' on any method; found: {string.Join(", ", offenders)}");
    }

    // -----------------------------------------------------------------------------------------
    // 1b. Contract test — IFacilityRepository must not expose ignoreQueryFilters parameter
    // -----------------------------------------------------------------------------------------

    [Fact]
    public void IFacilityRepository_DoesNotExposeIgnoreQueryFiltersParameter()
    {
        // FacilityEntity inherits ISoftDelete, so the global query filter is the single
        // source of truth for soft-delete visibility. No repository method should offer
        // a bypass parameter.
        var repositoryType = typeof(IFacilityRepository);

        var offenders = repositoryType.GetMethods()
            .SelectMany(m => m.GetParameters().Select(p => new { Method = m.Name, Param = p }))
            .Where(x => string.Equals(x.Param.Name, "ignoreQueryFilters", StringComparison.OrdinalIgnoreCase))
            .Select(x => $"{x.Method}({x.Param.ParameterType.Name} {x.Param.Name})")
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"IFacilityRepository must not expose an 'ignoreQueryFilters' parameter on any method " +
            $"(bypass surface); found: {string.Join(", ", offenders)}");

        // Sanity: Facility entity is ISoftDelete and AppDbContext configures a global query filter
        // on it — verified by AppDbContext_EverySoftDeletableEntity_HasGlobalQueryFilter above.
    }

    // -----------------------------------------------------------------------------------------
    // 1c. Contract test — IEnrollmentRepository must not expose ignoreQueryFilters parameter
    // -----------------------------------------------------------------------------------------

    [Fact]
    public void IEnrollmentRepository_DoesNotExposeIgnoreQueryFiltersParameter()
    {
        // Enrollment has a global query filter on Student.IsDeleted && Course.IsDeleted
        // (see EnrollmentConfiguration). No repository method should offer a bypass parameter.
        var repositoryType = typeof(IEnrollmentRepository);

        var offenders = repositoryType.GetMethods()
            .SelectMany(m => m.GetParameters().Select(p => new { Method = m.Name, Param = p }))
            .Where(x => string.Equals(x.Param.Name, "ignoreQueryFilters", StringComparison.OrdinalIgnoreCase))
            .Select(x => $"{x.Method}({x.Param.ParameterType.Name} {x.Param.Name})")
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"IEnrollmentRepository must not expose an 'ignoreQueryFilters' parameter on any method " +
            $"(bypass surface); found: {string.Join(", ", offenders)}");

        // Enrollment entity is ISoftDelete (via SoftDeletableEntity) and AppDbContext configures
        // a global query filter on it — verified by AppDbContext_EverySoftDeletableEntity_HasGlobalQueryFilter above.
    }

    // -----------------------------------------------------------------------------------------
    // 6b. EF model test — Enrollment global query filter references Student/Course, not its own IsDeleted
    // -----------------------------------------------------------------------------------------

    [Fact]
    public void AppDbContext_EnrollmentQueryFilter_ReferencesStudentAndCourseIsDeleted()
    {
        // KNOWN GAP: Enrollment inherits ISoftDelete and has its own IsDeleted column, but its
        // global query filter only checks Student.IsDeleted and Course.IsDeleted — it does NOT
        // filter on its own IsDeleted flag. This means a soft-deleted Enrollment that belongs to
        // an active Student and an active Course will NOT be filtered out by the global filter.
        //
        // This is a known security follow-up item. The current behavior relies on application
        // logic to exclude soft-deleted Enrollments. Removing IgnoreQueryFilters from the
        // EnrollmentRepository (commit 041) ensures no explicit bypass, but the gap in the
        // filter itself remains.
        using var context = CreateModelOnlyContext();

        var entityType = context.Model.FindEntityType(typeof(Enrollment));
        Assert.True(entityType is not null, "Enrollment is not mapped in AppDbContext.");

        var filters = entityType!.GetDeclaredQueryFilters()
            .Select(f => f.Expression)
            .Where(f => f is not null)
            .ToList();

        Assert.True(
            filters.Count > 0,
            "Enrollment has no global query filter; soft-deleted rows would leak into every query.");

        // Enrollment's filter must reference Student.IsDeleted and Course.IsDeleted via navigation
        Assert.True(
            filters.Any(f => ReferencesAnyMember(f!, "IsDeleted")),
            "Enrollment's global query filter must reference an IsDeleted property.");

        // Enrollment does NOT reference its own IsDeleted directly — document this known gap.
        Assert.False(
            filters.Any(f => ReferencesOwnMember(f!, "IsDeleted")),
            "Enrollment's global query filter should NOT reference its own IsDeleted (known gap). " +
            "The filter only checks Student.IsDeleted and Course.IsDeleted via navigation properties. " +
            "If this assertion breaks, the Enrollment filter has been updated to include its own IsDeleted — " +
            "remove this comment and update the test to Assert.True instead.");
    }

    // -----------------------------------------------------------------------------------------
    // 2. Contract test — IRepository<T> must not expose the filter-bypass query method
    // -----------------------------------------------------------------------------------------

    [Fact]
    public void IRepository_DoesNotExposeQueryWithIgnoreFilters()
    {
        var repositoryType = typeof(IRepository<>);

        var methodNames = repositoryType.GetMethods().Select(m => m.Name).ToList();

        Assert.DoesNotContain("QueryWithIgnoreFilters", methodNames, StringComparer.OrdinalIgnoreCase);

        // The default filtered query path must still exist — the fix removes the bypass, not the access.
        Assert.NotNull(repositoryType.GetMethod("Query"));

        var showDeletedParams = repositoryType.GetMethods()
            .SelectMany(m => m.GetParameters().Select(p => new { Method = m.Name, Param = p }))
            .Where(x => string.Equals(x.Param.Name, ShowDeletedParameter, StringComparison.OrdinalIgnoreCase))
            .Select(x => $"{x.Method}({x.Param.Name})")
            .ToList();

        Assert.True(
            showDeletedParams.Count == 0,
            $"IRepository<> must not expose a '{ShowDeletedParameter}' parameter on any method; found: {string.Join(", ", showDeletedParams)}");
    }

    // -----------------------------------------------------------------------------------------
    // 3. Contract tests — API controller list actions must not accept showDeleted
    // -----------------------------------------------------------------------------------------

    public static IEnumerable<object[]> FixedApiListControllers() => new[]
    {
        new object[] { typeof(StudentApiController) },
        new object[] { typeof(BranchApiController) },
        new object[] { typeof(TeachersApiController) },
        new object[] { typeof(CoursesApiController) },
    };

    [Theory]
    [MemberData(nameof(FixedApiListControllers))]
    public void ApiController_GetList_HasNoShowDeletedParameter(Type controllerType)
    {
        var getList = controllerType.GetMethod("GetList", BindingFlags.Public | BindingFlags.Instance);

        Assert.True(getList is not null, $"{controllerType.Name} must still expose a public GetList action.");

        var offenders = getList!.GetParameters()
            .Where(p => string.Equals(p.Name, ShowDeletedParameter, StringComparison.OrdinalIgnoreCase))
            .Select(p => $"{p.ParameterType.Name} {p.Name}")
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"{controllerType.Name}.GetList must not accept a '{ShowDeletedParameter}' query parameter; found: {string.Join(", ", offenders)}");
    }

    [Theory]
    [MemberData(nameof(FixedApiListControllers))]
    public void ApiController_NoActionAtAll_ExposesShowDeletedParameter(Type controllerType)
    {
        var offenders = controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetParameters().Select(p => new { Method = m.Name, Param = p }))
            .Where(x => string.Equals(x.Param.Name, ShowDeletedParameter, StringComparison.OrdinalIgnoreCase))
            .Select(x => $"{x.Method}({x.Param.ParameterType.Name} {x.Param.Name})")
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"{controllerType.Name} must not expose '{ShowDeletedParameter}' on any action; found: {string.Join(", ", offenders)}");
    }

    // -----------------------------------------------------------------------------------------
    // 4. Behavior tests — list services read through the filtered repository query path
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task StudentService_GetListAsync_ReturnsExactlyWhatFilteredRepositoryQueryYields()
    {
        var studentRepository = new Mock<IStudentRepository>();
        var mapper = new Mock<IMapper>();

        var activeStudents = new List<Student>
        {
            TestBase.CreateStudent(),
            TestBase.CreateStudent(),
            TestBase.CreateStudent(),
        };

        // The repository's default query path applies the global soft-delete filter; the service
        // must consume its result verbatim and must not express deleted-row visibility itself.
        var filteredPage = new Paginate<Student>(activeStudents, index: 0, size: 10, from: 0);

        Expression<Func<Student, bool>>? capturedPredicate = null;

        studentRepository
            .Setup(x => x.GetListAsync(
                It.IsAny<Expression<Func<Student, bool>>?>(),
                It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>?>(),
                It.IsAny<Func<IQueryable<Student>, IQueryable<Student>>?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Callback((Expression<Func<Student, bool>>? predicate, Func<IQueryable<Student>, IOrderedQueryable<Student>>? _,
                       Func<IQueryable<Student>, IQueryable<Student>>? __, int ___, int ____, bool _____,
                       CancellationToken ______) => capturedPredicate = predicate)
            .ReturnsAsync(filteredPage);

        mapper.Setup(x => x.Map<StudentListResponse>(It.IsAny<Student>()))
            .Returns((Student s) => new StudentListResponse
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                MobilePhone = s.MobilePhone,
                RegistrationDate = s.RegistrationDate,
                IsActive = s.IsActive
            });

        var service = new StudentService(
            studentRepository.Object,
            mapper.Object,
            NullLogger<StudentService>.Instance,
            TestBase.CreateValidator<CreateStudentRequest>().Object,
            TestBase.CreateValidator<UpdateStudentRequest>().Object);

        var result = await service.GetListAsync(
            new PageRequest { PageIndex = 0, PageSize = 10 },
            search: "test",
            isActive: true);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(3, result.Count);
        Assert.Equal(activeStudents.Select(s => s.Id), result.Items.Select(i => i.Id));

        studentRepository.Verify(
            x => x.GetListAsync(
                It.IsAny<Expression<Func<Student, bool>>?>(),
                It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>?>(),
                It.IsAny<Func<IQueryable<Student>, IQueryable<Student>>?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // The service must not re-implement deleted-row visibility in its own predicate;
        // the global query filter is the single source of truth.
        Assert.NotNull(capturedPredicate);
        Assert.False(
            ReferencesDeletedFlag(capturedPredicate!),
        "StudentService.GetListAsync must not reference IsDeleted in its repository predicate; " +
        "deleted-record visibility belongs exclusively to the global query filter.");
    }

    [Fact]
    public async Task BranchService_GetListAsync_ReturnsExactlyWhatFilteredRepositoryQueryYields()
    {
        var branchRepository = new Mock<IBranchRepository>();
        var mapper = new Mock<IMapper>();

        var activeBranches = new List<Branch>
        {
            new() { Id = Guid.NewGuid(), Name = "Branch A", Address = "Address A", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Branch B", Address = "Address B", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Branch C", Address = "Address C", IsActive = true },
        };

        var filteredPage = new Paginate<Branch>(activeBranches, index: 0, size: 10, from: 0);

        Expression<Func<Branch, bool>>? capturedPredicate = null;

        branchRepository
            .Setup(x => x.GetListAsync(
                It.IsAny<Expression<Func<Branch, bool>>?>(),
                It.IsAny<Func<IQueryable<Branch>, IOrderedQueryable<Branch>>?>(),
                It.IsAny<Func<IQueryable<Branch>, IQueryable<Branch>>?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Callback((Expression<Func<Branch, bool>>? predicate, Func<IQueryable<Branch>, IOrderedQueryable<Branch>>? _,
                       Func<IQueryable<Branch>, IQueryable<Branch>>? __, int ___, int ____, bool _____,
                       CancellationToken ______) => capturedPredicate = predicate)
            .ReturnsAsync(filteredPage);

        mapper.Setup(x => x.Map<BranchListResponse>(It.IsAny<Branch>()))
            .Returns((Branch b) => new BranchListResponse
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                PhoneNumber = b.PhoneNumber,
                IsActive = b.IsActive
            });

        var service = new BranchService(
            branchRepository.Object,
            mapper.Object,
            new Mock<IFacilityRepository>().Object,
            new Mock<IClassroomRepository>().Object,
            new Mock<ICourseRepository>().Object,
            new Mock<ITeacherRepository>().Object,
            new Mock<IOfferedLanguageRepository>().Object,
            NullLogger<BranchService>.Instance);

        var result = await service.GetListAsync(
            new PageRequest { PageIndex = 0, PageSize = 10 },
            search: "Branch",
            isActive: true);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(3, result.Count);
        Assert.Equal(activeBranches.Select(b => b.Id), result.Items.Select(i => i.Id));

        branchRepository.Verify(
            x => x.GetListAsync(
                It.IsAny<Expression<Func<Branch, bool>>?>(),
                It.IsAny<Func<IQueryable<Branch>, IOrderedQueryable<Branch>>?>(),
                It.IsAny<Func<IQueryable<Branch>, IQueryable<Branch>>?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.NotNull(capturedPredicate);
        Assert.False(
            ReferencesDeletedFlag(capturedPredicate!),
            "BranchService.GetListAsync must not reference IsDeleted in its repository predicate; " +
            "deleted-record visibility belongs exclusively to the global query filter.");
    }

    // -----------------------------------------------------------------------------------------
    // 5. Behavior test — a soft-deleted (invisible) record surfaces as NotFound from the service
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task StudentService_GetByIdAsync_WhenRecordIsInvisibleByFilter_ThrowsNotFound()
    {
        // The global query filter hides soft-deleted rows, so GetAsync returns null for them.
        // The service must translate that into NotFound (HTTP 404 at the API boundary) instead of
        // leaking or resurrecting the record. Mirrors StudentServiceTests.DeleteAsync_rejects_nonexistent_entity.
        var studentRepository = new Mock<IStudentRepository>();
        var mapper = new Mock<IMapper>();

        studentRepository.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Student, bool>>>(),
                It.IsAny<Func<IQueryable<Student>, IQueryable<Student>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        var service = new StudentService(
            studentRepository.Object,
            mapper.Object,
            NullLogger<StudentService>.Instance,
            TestBase.CreateValidator<CreateStudentRequest>().Object,
            TestBase.CreateValidator<UpdateStudentRequest>().Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));

        mapper.Verify(x => x.Map<StudentResponse>(It.IsAny<Student>()), Times.Never);
    }

    // -----------------------------------------------------------------------------------------
    // 6. EF model tests — the global soft-delete query filter is configured (no DB connection)
    // -----------------------------------------------------------------------------------------

    [Fact]
    public void AppDbContext_EverySoftDeletableEntity_HasGlobalQueryFilter()
    {
        using var context = CreateModelOnlyContext();

        var softDeletableEntityTypes = context.Model.GetEntityTypes()
            .Where(e => typeof(ISoftDelete).IsAssignableFrom(e.ClrType))
            .ToList();

        // Sanity check: all known soft-deletable entities are mapped.
        var mappedClrTypeNames = softDeletableEntityTypes.Select(e => e.ClrType.Name).ToList();
        Assert.Contains("Student", mappedClrTypeNames);
        Assert.Contains("Branch", mappedClrTypeNames);
        Assert.Contains("Teacher", mappedClrTypeNames);
        Assert.Contains("Course", mappedClrTypeNames);
        Assert.Contains("Classroom", mappedClrTypeNames);
        Assert.Contains("CourseLevel", mappedClrTypeNames);
        Assert.Contains("OfferedLanguage", mappedClrTypeNames);
        Assert.Contains("Facility", mappedClrTypeNames);
        Assert.Contains("Enrollment", mappedClrTypeNames);

        var entitiesWithoutFilter = softDeletableEntityTypes
            .Where(e => !e.GetDeclaredQueryFilters().Any())
            .Select(e => e.ClrType.Name)
            .ToList();

        Assert.True(
            entitiesWithoutFilter.Count == 0,
            "Every ISoftDelete entity must carry a global query filter; missing for: " +
            string.Join(", ", entitiesWithoutFilter));
    }

    [Fact]
    public void AppDbContext_CoreSoftDeletableEntities_FilterOnTheirOwnIsDeletedFlag()
    {
        using var context = CreateModelOnlyContext();

        // The 8 core entities behind the fixed list endpoints must be filtered by their own
        // IsDeleted flag (not only via a navigation to another entity).
        var coreEntities = new[]
        {
            typeof(Student), typeof(Branch), typeof(Teacher), typeof(Course),
            typeof(Classroom), typeof(CourseLevel), typeof(OfferedLanguage), typeof(Facility),
        };

        foreach (var clrType in coreEntities)
        {
            var entityType = context.Model.FindEntityType(clrType);
            Assert.True(entityType is not null, $"{clrType.Name} is not mapped in AppDbContext.");

            var filters = entityType!.GetDeclaredQueryFilters()
                .Select(f => f.Expression)
                .Where(f => f is not null)
                .ToList();

            Assert.True(
                filters.Count > 0,
                $"{clrType.Name} has no global query filter; soft-deleted rows would leak into every query.");

            Assert.True(
                filters.Any(f => ReferencesOwnMember(f!, "IsDeleted")),
                $"{clrType.Name}'s global query filter must reference its own IsDeleted property; " +
                $"actual filters: {string.Join(" | ", filters)}");
        }
    }

    // -----------------------------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------------------------

    /// <summary>
    /// Builds the EF model for <see cref="AppDbContext"/> without ever connecting to a database.
    /// Model building only parses the (placeholder) connection string; no query is executed here.
    /// </summary>
    private static AppDbContext CreateModelOnlyContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost;Database=lang-course-security-model-check;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new AppDbContext(options);
    }

    private static bool ReferencesDeletedFlag(LambdaExpression lambda) => ReferencesAnyMember(lambda, "IsDeleted");

    private static bool ReferencesAnyMember(LambdaExpression lambda, string memberName)
    {
        var visitor = new MemberNameVisitor(memberName);
        visitor.Visit(lambda.Body);
        return visitor.Found;
    }

    /// <summary>True when the lambda accesses <paramref name="memberName"/> directly on its own parameter.</summary>
    private static bool ReferencesOwnMember(LambdaExpression lambda, string memberName)
    {
        var visitor = new OwnMemberAccessVisitor(lambda.Parameters[0], memberName);
        visitor.Visit(lambda.Body);
        return visitor.Found;
    }

    private sealed class MemberNameVisitor : ExpressionVisitor
    {
        private readonly string _memberName;

        public MemberNameVisitor(string memberName) => _memberName = memberName;

        public bool Found { get; private set; }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.Name == _memberName)
                Found = true;

            return base.VisitMember(node);
        }
    }

    private sealed class OwnMemberAccessVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        private readonly string _memberName;

        public OwnMemberAccessVisitor(ParameterExpression parameter, string memberName)
        {
            _parameter = parameter;
            _memberName = memberName;
        }

        public bool Found { get; private set; }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.Name == _memberName && node.Expression == _parameter)
                Found = true;

            return base.VisitMember(node);
        }
    }
}

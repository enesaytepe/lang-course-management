using System.Collections;
using System.Linq.Expressions;
using AutoMapper;
using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Classrooms;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Mapping;
using LanguageCourseManagement.Application.Services.ClassroomService;
using LanguageCourseManagement.Application.Validators;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests.ClassroomRules;

public sealed class ClassroomBusinessRulesTests
{
    [Fact]
    public void CreateValidator_rejects_whitespace_name_and_non_positive_capacity()
    {
        var result = new CreateClassroomRequestValidator().Validate(new CreateClassroomRequest
        {
            BranchId = Guid.NewGuid(),
            Name = "   ",
            Capacity = 0
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClassroomRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClassroomRequest.Capacity));
    }

    [Fact]
    public async Task Create_rejects_invalid_request_before_branch_or_persistence_access()
    {
        var classroomRepository = new Mock<IClassroomRepository>();
        var branchRepository = new Mock<IBranchRepository>();
        var service = CreateService(classroomRepository, branchRepository, Guid.NewGuid(), true);

        var exception = await Assert.ThrowsAsync<LanguageCourseManagement.Application.Exceptions.ValidationException>(
            () => service.CreateAsync(new CreateClassroomRequest
            {
                BranchId = Guid.Empty,
                Name = "   ",
                Description = new string('x', 501),
                Capacity = 0
            }));

        Assert.Contains(exception.Errors, error => error.Property == nameof(CreateClassroomRequest.Name));
        Assert.Contains(exception.Errors, error => error.Property == nameof(CreateClassroomRequest.Description));
        Assert.Contains(exception.Errors, error => error.Property == nameof(CreateClassroomRequest.Capacity));
        branchRepository.VerifyNoOtherCalls();
        classroomRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_rejects_invalid_request_before_classroom_lookup()
    {
        var classroomRepository = new Mock<IClassroomRepository>();
        var branchRepository = new Mock<IBranchRepository>();
        var service = CreateService(classroomRepository, branchRepository, Guid.NewGuid(), true);

        var exception = await Assert.ThrowsAsync<LanguageCourseManagement.Application.Exceptions.ValidationException>(
            () => service.UpdateAsync(Guid.NewGuid(), new UpdateClassroomRequest
            {
                BranchId = Guid.Empty,
                Name = "",
                Description = new string('x', 501),
                Capacity = -1,
                IsActive = true
            }));

        Assert.Contains(exception.Errors, error => error.Property == nameof(UpdateClassroomRequest.Name));
        Assert.Contains(exception.Errors, error => error.Property == nameof(UpdateClassroomRequest.Description));
        Assert.Contains(exception.Errors, error => error.Property == nameof(UpdateClassroomRequest.Capacity));
        classroomRepository.VerifyNoOtherCalls();
        branchRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_rejects_duplicate_name_while_excluding_current_classroom()
    {
        var classroomId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var classroomRepository = new Mock<IClassroomRepository>();
        classroomRepository
            .Setup(repository => repository.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Classroom, bool>>>(),
                It.IsAny<Func<IQueryable<Classroom>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Classroom, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Classroom { Id = classroomId, BranchId = branchId, Name = "A", Capacity = 10 });
        classroomRepository
            .Setup(repository => repository.NameExistsAsync(
                branchId,
                "B",
                classroomId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService(classroomRepository, branchId, active: true);

        await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(
            classroomId,
            new UpdateClassroomRequest
            {
                BranchId = branchId,
                Name = "B",
                Capacity = 10,
                IsActive = true
            }));

        classroomRepository.Verify(repository => repository.NameExistsAsync(
            branchId, "B", classroomId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_rejects_inactive_branch()
    {
        var classroomRepository = new Mock<IClassroomRepository>();
        var branchId = Guid.NewGuid();
        var service = CreateService(classroomRepository, branchId, active: false);

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(new CreateClassroomRequest
        {
            BranchId = branchId,
            Name = "Room 1",
            Capacity = 10
        }));

        classroomRepository.Verify(repository => repository.NameExistsAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_rejects_duplicate_name_in_same_branch()
    {
        var branchId = Guid.NewGuid();
        var classroomRepository = new Mock<IClassroomRepository>();
        classroomRepository
            .Setup(repository => repository.NameExistsAsync(
                branchId,
                "Room 1",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService(classroomRepository, branchId, active: true);

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(new CreateClassroomRequest
        {
            BranchId = branchId,
            Name = " Room 1 ",
            Capacity = 10
        }));

        classroomRepository.Verify(repository => repository.NameExistsAsync(
            branchId, "Room 1", null, It.IsAny<CancellationToken>()), Times.Once);
        classroomRepository.Verify(repository => repository.AddAsync(It.IsAny<Classroom>()), Times.Never);
    }

    [Fact]
    public async Task Create_allows_reusing_name_after_soft_deleted_classroom_is_hidden()
    {
        var branchId = Guid.NewGuid();
        var classroomRepository = new Mock<IClassroomRepository>();
        classroomRepository
            .Setup(repository => repository.NameExistsAsync(
                branchId,
                "Room 1",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // CreateAsync generates a new Id then calls GetByIdAsync which uses Query().ProjectTo().
        // Use a shared list populated via AddAsync callback so Query() returns the right data.
        var storedClassrooms = new List<Classroom>();
        classroomRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Classroom>(), It.IsAny<CancellationToken>()))
            .Callback<Classroom, CancellationToken>((c, _) =>
            {
                c.Branch = new Branch { Id = branchId, Name = "Central", IsActive = true };
                storedClassrooms.Add(c);
            })
            .ReturnsAsync((Classroom c, CancellationToken _) => c);
        classroomRepository
            .Setup(repository => repository.Query())
            .Returns(() => ToAsyncQueryable(storedClassrooms));

        var service = CreateService(classroomRepository, branchId, active: true);
        var response = await service.CreateAsync(new CreateClassroomRequest
        {
            BranchId = branchId,
            Name = " Room 1 ",
            Capacity = 10
        });

        Assert.NotNull(response);
        Assert.Equal(branchId, response.BranchId);
        Assert.Equal("Room 1", response.Name);
        Assert.Equal(10, response.Capacity);
        Assert.True(response.IsActive);
        classroomRepository.Verify(repository => repository.NameExistsAsync(
            branchId, "Room 1", null, It.IsAny<CancellationToken>()), Times.Once);
        classroomRepository.Verify(repository => repository.AddAsync(It.Is<Classroom>(classroom =>
            classroom.BranchId == branchId && classroom.Name == "Room 1" && classroom.IsActive)), Times.Once);
    }

    [Fact]
    public async Task Update_allows_retaining_an_inactive_branch_but_rejects_moving_to_one()
    {
        var classroomId = Guid.NewGuid();
        var originalBranchId = Guid.NewGuid();
        var inactiveDestinationId = Guid.NewGuid();
        var classroomRepository = new Mock<IClassroomRepository>();
        classroomRepository
            .Setup(repository => repository.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Classroom, bool>>>(),
                It.IsAny<Func<IQueryable<Classroom>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Classroom, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Classroom { Id = classroomId, BranchId = originalBranchId, Name = "A", Capacity = 10 });
        var branchRepository = new Mock<IBranchRepository>();
        branchRepository
            .Setup(repository => repository.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>(),
                It.IsAny<Func<IQueryable<Branch>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Branch, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Branch, bool>> predicate, Func<IQueryable<Branch>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Branch, object>>? _, bool _, CancellationToken _) =>
            {
                var branch = new Branch { Id = originalBranchId, IsActive = false };
                return predicate.Compile()(branch) ? branch : null;
            });
        classroomRepository.Setup(repository => repository.NameExistsAsync(
                originalBranchId, "A", classroomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // UpdateAsync calls GetByIdAsync at the end, which uses Query().ProjectTo().
        classroomRepository
            .Setup(repository => repository.Query())
            .Returns(() => ToAsyncQueryable(new[]
            {
                new Classroom
                {
                    Id = classroomId,
                    BranchId = originalBranchId,
                    Name = "A",
                    Capacity = 10,
                    Branch = new Branch { Id = originalBranchId, Name = "Central", IsActive = false }
                }
            }));

        var service = CreateService(classroomRepository, branchRepository, branchId: originalBranchId, active: false);
        var response = await service.UpdateAsync(classroomId, new UpdateClassroomRequest
        {
            BranchId = originalBranchId,
            Name = "A",
            Capacity = 10,
            IsActive = true
        });

        Assert.Equal(classroomId, response.Id);
        await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(classroomId, new UpdateClassroomRequest
        {
            BranchId = inactiveDestinationId,
            Name = "A",
            Capacity = 10,
            IsActive = true
        }));
    }

    [Fact]
    public async Task Delete_delegates_soft_delete_and_returns_pre_delete_response()
    {
        var classroom = new Classroom
        {
            Id = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Name = "Room 1",
            Capacity = 12,
            IsActive = true,
            Branch = new Branch { Id = Guid.NewGuid(), Name = "Central", IsActive = true }
        };
        var classroomRepository = new Mock<IClassroomRepository>();
        classroomRepository
            .Setup(repository => repository.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Classroom, bool>>>(),
                It.IsAny<Func<IQueryable<Classroom>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Classroom, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(classroom);
        classroomRepository
            .Setup(repository => repository.DeleteAsync(classroom))
            .Callback(() => { classroom.IsDeleted = true; classroom.DeletedAt = DateTimeOffset.UtcNow; })
            .ReturnsAsync(classroom);

        // DeleteAsync calls Query().Where().ProjectTo() for the pre-delete response.
        classroomRepository
            .Setup(repository => repository.Query())
            .Returns(() => ToAsyncQueryable(new[] { classroom }));

        var service = CreateService(classroomRepository, classroom.BranchId, active: true);
        var response = await service.DeleteAsync(classroom.Id);

        Assert.Equal(classroom.Id, response.Id);
        Assert.True(classroom.IsDeleted);
        Assert.NotNull(classroom.DeletedAt);
        classroomRepository.Verify(repository => repository.DeleteAsync(classroom), Times.Once);
    }

    [Fact]
    public async Task Delete_rejects_repeated_delete_after_query_filter_hides_classroom()
    {
        var classroom = new Classroom
        {
            Id = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Name = "Room 1",
            Capacity = 12,
            IsActive = true,
            Branch = new Branch { Id = Guid.NewGuid(), Name = "Central", IsActive = true }
        };
        var classroomRepository = new Mock<IClassroomRepository>();
        classroomRepository
            .Setup(repository => repository.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Classroom, bool>>>(),
                It.IsAny<Func<IQueryable<Classroom>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Classroom, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(classroom)
            .Callback(() => classroom.IsDeleted = true);
        classroomRepository
            .Setup(repository => repository.DeleteAsync(classroom))
            .ReturnsAsync(classroom);

        // First DeleteAsync call uses Query().Where().ProjectTo() for the pre-delete response.
        classroomRepository
            .Setup(repository => repository.Query())
            .Returns(() => ToAsyncQueryable(new[] { classroom }));

        var service = CreateService(classroomRepository, classroom.BranchId, active: true);

        await service.DeleteAsync(classroom.Id);

        classroomRepository.Reset();
        classroomRepository
            .Setup(repository => repository.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Classroom, bool>>>(),
                It.IsAny<Func<IQueryable<Classroom>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Classroom, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Classroom?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(classroom.Id));

        classroomRepository.Verify(repository => repository.DeleteAsync(It.IsAny<Classroom>()), Times.Never);
    }

    private static ClassroomService CreateService(
        Mock<IClassroomRepository> classroomRepository,
        Guid branchId,
        bool active)
    {
        return CreateService(classroomRepository, new Mock<IBranchRepository>(), branchId, active);
    }

    private static ClassroomService CreateService(
        Mock<IClassroomRepository> classroomRepository,
        Mock<IBranchRepository> branchRepository,
        Guid branchId,
        bool active)
    {
        branchRepository
            .Setup(repository => repository.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>(),
                It.IsAny<Func<IQueryable<Branch>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Branch, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch { Id = branchId, Name = "Central", IsActive = active });

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ClassroomProfile>();
            cfg.AddProfile<BranchProfile>();
        }, NullLoggerFactory.Instance);
        var mapper = mapperConfig.CreateMapper();

        return new ClassroomService(
            classroomRepository.Object,
            branchRepository.Object,
            mapper,
            NullLogger<ClassroomService>.Instance,
            new CreateClassroomRequestValidator(),
            new UpdateClassroomRequestValidator());
    }

    // --- AsyncQueryable helpers so FirstOrDefaultAsync works on in-memory IQueryable ---

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
            // EF Core calls ExecuteAsync<TResult> where TResult is ValueTask<TSource> or Task<TSource>.
            // Evaluate the LINQ expression synchronously against in-memory data, then wrap.
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

            // Compile and execute the expression (scalar result, e.g. ClassroomResponse)
            var lambda = Expression.Lambda<Func<object>>(
                Expression.Convert(expression, typeof(object)));
            var result = lambda.Compile()();

            if (valueType != null)
            {
                if (isTask)
                {
                    // Task.FromResult<T>(value) via reflection
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

using AutoMapper;
using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Classrooms;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Mapping;
using LanguageCourseManagement.Application.Services.ClassroomService;
using LanguageCourseManagement.Application.Validators;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
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
        var deletedClassroom = new Classroom
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            Name = "Room 1",
            Capacity = 10,
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow
        };
        var classroomRepository = new Mock<IClassroomRepository>();
        classroomRepository
            .Setup(repository => repository.NameExistsAsync(
                branchId,
                "Room 1",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        classroomRepository
            .Setup(repository => repository.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Classroom, bool>>>(),
                It.IsAny<Func<IQueryable<Classroom>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Classroom, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedClassroom);

        var service = CreateService(classroomRepository, branchId, active: true);
        var response = await service.CreateAsync(new CreateClassroomRequest
        {
            BranchId = branchId,
            Name = " Room 1 ",
            Capacity = 10
        });

        Assert.Equal(deletedClassroom.Id, response.Id);
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

        var mapper = new Mock<IMapper>();
        mapper.Setup(value => value.Map<Classroom>(It.IsAny<CreateClassroomRequest>()))
            .Returns((CreateClassroomRequest request) => new Classroom
            {
                BranchId = request.BranchId,
                Name = request.Name,
                Description = request.Description,
                Capacity = request.Capacity
            });
        mapper.Setup(value => value.Map<ClassroomResponse>(It.IsAny<object>()))
            .Returns((object source) =>
            {
                var classroom = (Classroom)source;
                return new ClassroomResponse
                {
                    Id = classroom.Id,
                    BranchId = classroom.BranchId,
                    BranchName = classroom.Branch?.Name ?? string.Empty,
                    Name = classroom.Name,
                    Description = classroom.Description,
                    Capacity = classroom.Capacity,
                    IsActive = classroom.IsActive
                };
            });

        return new ClassroomService(
            classroomRepository.Object,
            branchRepository.Object,
            mapper.Object,
            NullLogger<ClassroomService>.Instance,
            new CreateClassroomRequestValidator(),
            new UpdateClassroomRequestValidator());
    }
}

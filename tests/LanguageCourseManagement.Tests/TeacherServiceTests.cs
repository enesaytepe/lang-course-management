using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using LanguageCourseManagement.Application.DTOs.Teachers;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.TeacherService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class TeacherServiceTests
{
    private readonly Mock<ITeacherRepository> teacherRepository = new();
    private readonly Mock<ICourseRepository> courseRepository = new();
    private readonly Mock<IOfferedLanguageRepository> languageRepository = new();
    private readonly Mock<IBranchRepository> branchRepository = new();
    private readonly Mock<ICourseLevelRepository> courseLevelRepository = new();
    private readonly Mock<IMapper> mapper = new();
    private readonly Mock<IValidator<CreateTeacherRequest>> createValidator = new();
    private readonly Mock<IValidator<UpdateTeacherRequest>> updateValidator = new();
    private readonly Mock<IValidator<CreateTeacherAvailabilityRequest>> createAvailValidator = new();
    private readonly Mock<IValidator<UpdateTeacherAvailabilityRequest>> updateAvailValidator = new();

    [Fact]
    public async Task CreateAsync_rejects_empty_language_ids()
    {
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateTeacherRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var service = CreateService();
        var request = new CreateTeacherRequest
        {
            FirstName = "Test",
            LastName = "Teacher",
            MobilePhone = "05000000000",
            LanguageIds = new List<Guid>(),
            BranchIds = new List<Guid> { Guid.NewGuid() }
        };

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task DeleteAsync_rejects_nonexistent_entity()
    {
        teacherRepository.Setup(x => x.GetByIdWithDetailsForMutationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Teacher?)null);

        var service = CreateService();
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    private TeacherService CreateService()
    {
        return new TeacherService(
            teacherRepository.Object,
            courseRepository.Object,
            languageRepository.Object,
            branchRepository.Object,
            courseLevelRepository.Object,
            mapper.Object,
            NullLogger<TeacherService>.Instance,
            createValidator.Object,
            updateValidator.Object,
            createAvailValidator.Object,
            updateAvailValidator.Object);
    }
}

using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using LanguageCourseManagement.Application.DTOs.CourseLevels;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.CourseLevelService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class CourseLevelServiceTests
{
    private readonly Mock<ICourseLevelRepository> levelRepository = new();
    private readonly Mock<IOfferedLanguageRepository> languageRepository = new();
    private readonly Mock<IMapper> mapper = new();
    private readonly Mock<IValidator<CreateCourseLevelRequest>> createValidator = new();
    private readonly Mock<IValidator<UpdateCourseLevelRequest>> updateValidator = new();

    [Fact]
    public async Task CreateAsync_rejects_duplicate_name()
    {
        var languageId = Guid.NewGuid();
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateCourseLevelRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        languageRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<OfferedLanguage, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<OfferedLanguage>, System.Linq.IQueryable<OfferedLanguage>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OfferedLanguage { Id = languageId, IsActive = true });

        levelRepository.Setup(x => x.NameExistsAsync(languageId, "A1", It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var request = new CreateCourseLevelRequest { OfferedLanguageId = languageId, Name = "A1", Order = 1 };

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task DeleteAsync_rejects_nonexistent_entity()
    {
        levelRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CourseLevel, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<CourseLevel>, System.Linq.IQueryable<CourseLevel>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((CourseLevel?)null);

        var service = CreateService();
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    private CourseLevelService CreateService()
    {
        return new CourseLevelService(
            levelRepository.Object,
            languageRepository.Object,
            mapper.Object,
            NullLogger<CourseLevelService>.Instance,
            createValidator.Object,
            updateValidator.Object);
    }
}

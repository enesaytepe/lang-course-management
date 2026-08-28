using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using LanguageCourseManagement.Application.DTOs.OfferedLanguages;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.OfferedLanguageService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class OfferedLanguageServiceTests
{
    private readonly Mock<IOfferedLanguageRepository> languageRepository = new();
    private readonly Mock<IMapper> mapper = new();
    private readonly Mock<IValidator<CreateOfferedLanguageRequest>> createValidator = new();
    private readonly Mock<IValidator<UpdateOfferedLanguageRequest>> updateValidator = new();

    [Fact]
    public async Task CreateAsync_rejects_duplicate_name()
    {
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateOfferedLanguageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        languageRepository.Setup(x => x.NameExistsAsync("English", It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var request = new CreateOfferedLanguageRequest { Name = "English", Code = "EN" };

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task DeleteAsync_rejects_nonexistent_entity()
    {
        languageRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<OfferedLanguage, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<OfferedLanguage>, System.Linq.IQueryable<OfferedLanguage>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((OfferedLanguage?)null);

        var service = CreateService();
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    private OfferedLanguageService CreateService()
    {
        return new OfferedLanguageService(
            languageRepository.Object,
            mapper.Object,
            NullLogger<OfferedLanguageService>.Instance,
            createValidator.Object,
            updateValidator.Object);
    }
}

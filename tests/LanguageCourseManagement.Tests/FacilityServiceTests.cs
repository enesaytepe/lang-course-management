using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using LanguageCourseManagement.Application.DTOs.Facilities;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.FacilityService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class FacilityServiceTests
{
    private readonly Mock<IFacilityRepository> facilityRepository = new();
    private readonly Mock<IMapper> mapper = new();
    private readonly Mock<IValidator<CreateFacilityRequest>> createValidator = new();
    private readonly Mock<IValidator<UpdateFacilityRequest>> updateValidator = new();

    [Fact]
    public async Task CreateAsync_rejects_duplicate_name()
    {
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateFacilityRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        facilityRepository.Setup(x => x.NameExistsAsync("Parking", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var request = new CreateFacilityRequest { Name = "Parking", IsActive = true };

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task DeleteAsync_rejects_nonexistent_entity()
    {
        facilityRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var service = CreateService();
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    private FacilityService CreateService()
    {
        return new FacilityService(
            facilityRepository.Object,
            mapper.Object,
            NullLogger<FacilityService>.Instance,
            createValidator.Object,
            updateValidator.Object);
    }
}

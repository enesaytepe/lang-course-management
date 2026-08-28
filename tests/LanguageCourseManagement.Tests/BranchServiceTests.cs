using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Branches;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.BranchService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class BranchServiceTests
{
    private readonly Mock<IBranchRepository> branchRepository = new();
    private readonly Mock<IMapper> mapper = new();
    private readonly Mock<IFacilityRepository> facilityRepository = new();
    private readonly Mock<IClassroomRepository> classroomRepository = new();
    private readonly Mock<ICourseRepository> courseRepository = new();
    private readonly Mock<ITeacherRepository> teacherRepository = new();
    private readonly Mock<IOfferedLanguageRepository> languageRepository = new();

    [Fact]
    public async Task CreateAsync_rejects_duplicate_name()
    {
        branchRepository.Setup(x => x.NameExistsAsync("Branch A", It.IsAny<Guid?>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var request = new CreateBranchRequest { Name = "Branch A", Address = "Address 1" };

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task DeleteAsync_rejects_nonexistent_entity()
    {
        branchRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Branch, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<Branch>, System.Linq.IQueryable<Branch>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        var service = CreateService();
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    private BranchService CreateService()
    {
        return new BranchService(
            branchRepository.Object,
            mapper.Object,
            facilityRepository.Object,
            classroomRepository.Object,
            courseRepository.Object,
            teacherRepository.Object,
            languageRepository.Object,
            NullLogger<BranchService>.Instance);
    }
}

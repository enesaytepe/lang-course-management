using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using LanguageCourseManagement.Application.DTOs.Students;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.StudentService;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class StudentServiceTests
{
    private readonly Mock<IStudentRepository> studentRepository = new();
    private readonly Mock<IMapper> mapper = new();
    private readonly Mock<IValidator<CreateStudentRequest>> createValidator = new();
    private readonly Mock<IValidator<UpdateStudentRequest>> updateValidator = new();

    [Fact]
    public async Task CreateAsync_rejects_invalid_request()
    {
        createValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateStudentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("FirstName", "Ad zorunludur.")
            }));

        var service = CreateService();
        var request = new CreateStudentRequest { FirstName = "", LastName = "Student", MobilePhone = "05000000000" };

        await Assert.ThrowsAsync<LanguageCourseManagement.Application.Exceptions.ValidationException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task DeleteAsync_rejects_nonexistent_entity()
    {
        studentRepository.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Student, bool>>>(),
            It.IsAny<Func<System.Linq.IQueryable<Student>, System.Linq.IQueryable<Student>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        var service = CreateService();
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    private StudentService CreateService()
    {
        return new StudentService(
            studentRepository.Object,
            mapper.Object,
            NullLogger<StudentService>.Instance,
            createValidator.Object,
            updateValidator.Object);
    }
}

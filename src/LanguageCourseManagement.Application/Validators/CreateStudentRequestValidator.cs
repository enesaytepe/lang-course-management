using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Students;

namespace LanguageCourseManagement.Application.Validators;

public sealed class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(request => request.FirstName)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .MaximumLength(100);
        RuleFor(request => request.LastName)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .MaximumLength(100);
        RuleFor(request => request.MobilePhone).NotEmpty().MaximumLength(20);
        RuleFor(request => request.HomePhone).MaximumLength(20);
        RuleFor(request => request.Email)
            .EmailAddress()
            .Unless(request => string.IsNullOrWhiteSpace(request.Email))
            .MaximumLength(200);
    }
}

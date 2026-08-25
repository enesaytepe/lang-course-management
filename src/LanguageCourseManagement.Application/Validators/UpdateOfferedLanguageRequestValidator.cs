using FluentValidation;
using LanguageCourseManagement.Application.DTOs.OfferedLanguages;

namespace LanguageCourseManagement.Application.Validators;

public sealed class UpdateOfferedLanguageRequestValidator : AbstractValidator<UpdateOfferedLanguageRequest>
{
    public UpdateOfferedLanguageRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .MaximumLength(100);
        RuleFor(request => request.Code).MaximumLength(10);
        RuleFor(request => request.IsActive).NotNull();
    }
}

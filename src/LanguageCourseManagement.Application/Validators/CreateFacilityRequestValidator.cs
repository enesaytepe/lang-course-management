using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Facilities;

namespace LanguageCourseManagement.Application.Validators;

public sealed class CreateFacilityRequestValidator : AbstractValidator<CreateFacilityRequest>
{
    public CreateFacilityRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(1000);
    }
}

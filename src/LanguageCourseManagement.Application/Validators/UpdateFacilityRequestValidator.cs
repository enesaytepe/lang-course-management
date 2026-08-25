using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Facilities;

namespace LanguageCourseManagement.Application.Validators;

public sealed class UpdateFacilityRequestValidator : AbstractValidator<UpdateFacilityRequest>
{
    public UpdateFacilityRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(1000);
    }
}

using FluentValidation;
using LanguageCourseManagement.Application.DTOs.CourseLevels;

namespace LanguageCourseManagement.Application.Validators;

public sealed class CreateCourseLevelRequestValidator : AbstractValidator<CreateCourseLevelRequest>
{
    public CreateCourseLevelRequestValidator()
    {
        RuleFor(request => request.OfferedLanguageId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().Must(name => !string.IsNullOrWhiteSpace(name)).MaximumLength(50);
        RuleFor(request => request.Description).MaximumLength(500);
        RuleFor(request => request.Order).GreaterThanOrEqualTo(0);
    }
}
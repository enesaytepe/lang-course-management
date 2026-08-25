using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Teachers;

namespace LanguageCourseManagement.Application.Validators;

public sealed class CreateTeacherAvailabilityRequestValidator : AbstractValidator<CreateTeacherAvailabilityRequest>
{
    public CreateTeacherAvailabilityRequestValidator()
    {
        RuleFor(request => request.DayOfWeek).IsInEnum();
        RuleFor(request => request.StartTime).NotEmpty();
        RuleFor(request => request.EndTime).NotEmpty();
        RuleFor(request => request.StartTime).LessThan(request => request.EndTime)
            .WithMessage("Başlangıç saati bitiş saatinden küçük olmalıdır.");
    }
}

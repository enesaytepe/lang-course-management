using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Classrooms;

namespace LanguageCourseManagement.Application.Validators;

public sealed class CreateClassroomRequestValidator : AbstractValidator<CreateClassroomRequest>
{
    public CreateClassroomRequestValidator()
    {
        RuleFor(request => request.BranchId).NotEmpty();
        RuleFor(request => request.Name)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .MaximumLength(100);
        RuleFor(request => request.Description).MaximumLength(500);
        RuleFor(request => request.Capacity).GreaterThan(0);
    }
}
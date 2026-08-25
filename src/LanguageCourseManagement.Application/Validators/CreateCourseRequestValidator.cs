using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Courses;

namespace LanguageCourseManagement.Application.Validators;

public sealed class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .MaximumLength(150);
        RuleFor(request => request.BranchId).NotEmpty();
        RuleFor(request => request.OfferedLanguageId).NotEmpty();
        RuleFor(request => request.CourseLevelId).NotEmpty();
        RuleFor(request => request.TeacherId).NotEmpty();
        RuleFor(request => request.ClassroomId).NotEmpty();
        RuleFor(request => request.StartDate).NotEmpty();
        RuleFor(request => request.EndDate)
            .NotEmpty()
            .GreaterThan(request => request.StartDate);
        RuleFor(request => request.Capacity).GreaterThan(0);
        RuleFor(request => request.TuitionFee).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Status).IsInEnum();
    }
}

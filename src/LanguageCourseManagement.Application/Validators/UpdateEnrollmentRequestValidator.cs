using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.Validators;

public sealed class UpdateEnrollmentRequestValidator : AbstractValidator<UpdateEnrollmentRequest>
{
    public UpdateEnrollmentRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => status is EnrollmentStatus.Completed or EnrollmentStatus.Cancelled)
            .WithMessage("Kayıt yalnızca tamamlandı veya iptal edildi durumuna alınabilir.");
    }
}

using FluentValidation;
using LanguageCourseManagement.Application.Common.Requests;

namespace LanguageCourseManagement.Application.Common.Requests.Validators;

public class PageRequestValidator : AbstractValidator<PageRequest>
{
    public PageRequestValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}

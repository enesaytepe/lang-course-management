using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Exceptions.HttpProblemDetails;

/// <summary>
/// 409 Conflict iş kuralı hatası için ProblemDetails yanıtı.
/// </summary>
public class BusinessProblemDetails : ProblemDetails
{
    public BusinessProblemDetails(string detail)
    {
        Title = "Rule violation";
        Detail = detail;
        Status = StatusCodes.Status409Conflict;
        Type = "https://api.languagemanagement.edu.tr/problems/business-rule-violation";
    }
}

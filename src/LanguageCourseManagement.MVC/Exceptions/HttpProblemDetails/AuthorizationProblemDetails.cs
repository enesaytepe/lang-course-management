using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Exceptions.HttpProblemDetails;

/// <summary>
/// 403 Forbidden hatası için ProblemDetails yanıtı.
/// </summary>
public class AuthorizationProblemDetails : ProblemDetails
{
    public AuthorizationProblemDetails(string detail)
    {
        Title = "Authorization error";
        Detail = detail;
        Status = StatusCodes.Status403Forbidden;
        Type = "https://api.languagemanagement.edu.tr/problems/authorization";
    }
}

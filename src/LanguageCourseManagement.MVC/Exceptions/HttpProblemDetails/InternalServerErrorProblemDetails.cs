using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Exceptions.HttpProblemDetails;

/// <summary>
/// 500 Internal Server Error için ProblemDetails yanıtı.
/// </summary>
public class InternalServerErrorProblemDetails : ProblemDetails
{
    public InternalServerErrorProblemDetails()
    {
        Title = "Internal server error";
        Detail = "Internal server error";
        Status = StatusCodes.Status500InternalServerError;
        Type = "https://api.languagemanagement.edu.tr/problems/internal-server-error";
    }
}

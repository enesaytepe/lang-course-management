using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Exceptions.HttpProblemDetails;

/// <summary>
/// 404 Not Found hatası için ProblemDetails yanıtı.
/// </summary>
public class NotFoundProblemDetails : ProblemDetails
{
    public NotFoundProblemDetails(string detail)
    {
        Title = "Not found";
        Detail = detail;
        Status = StatusCodes.Status404NotFound;
        Type = "https://example.com/probs/notfound";
    }
}

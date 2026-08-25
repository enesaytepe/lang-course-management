namespace LanguageCourseManagement.Application.Exceptions;

/// <summary>
/// İş kuralı ihlali durumunda fırlatılan exception. 400 HTTP yanıtına eşlenir.
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// Yeni bir <see cref="BusinessException"/> oluşturur.
    /// </summary>
    public BusinessException(string message)
        : base(message) { }
}

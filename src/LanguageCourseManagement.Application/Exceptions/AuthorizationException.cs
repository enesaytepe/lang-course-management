namespace LanguageCourseManagement.Application.Exceptions;

/// <summary>
/// Yetki ihlali durumunda fırlatılan exception. 403 HTTP yanıtına eşlenir.
/// </summary>
public class AuthorizationException : Exception
{
    /// <summary>
    /// Yeni bir <see cref="AuthorizationException"/> oluşturur.
    /// </summary>
    public AuthorizationException(string message)
        : base(message) { }
}

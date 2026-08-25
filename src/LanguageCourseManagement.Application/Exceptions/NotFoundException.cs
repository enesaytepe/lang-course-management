namespace LanguageCourseManagement.Application.Exceptions;

/// <summary>
/// Kaynak bulunamadığında fırlatılan exception. 404 HTTP yanıtına eşlenir.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Yeni bir <see cref="NotFoundException"/> oluşturur.
    /// </summary>
    public NotFoundException(string message)
        : base(message) { }
}

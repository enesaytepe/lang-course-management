namespace LanguageCourseManagement.Application.Exceptions;

/// <summary>
/// Doğrulama hatası durumunda fırlatılan exception. 400 HTTP yanıtına eşlenir.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Doğrulama hata detayları.
    /// </summary>
    public IEnumerable<ValidationExceptionModel> Errors { get; }

    /// <summary>
    /// Yeni bir <see cref="ValidationException"/> doğrulama hatalarıyla oluşturur.
    /// </summary>
    public ValidationException(IEnumerable<ValidationExceptionModel> errors)
        : base(BuildErrorMessage(errors))
    {
        Errors = errors;
    }

    private static string BuildErrorMessage(IEnumerable<ValidationExceptionModel> errors)
    {
        IEnumerable<string> arr = errors.Select(
            x => $"{Environment.NewLine} -- {x.Property}: {string.Join(Environment.NewLine, x.Errors ?? Enumerable.Empty<string>())}"
        );
        return $"Validation failed: {string.Join(string.Empty, arr)}";
    }
}

/// <summary>
/// Tek bir alanın doğrulama hata bilgilerini taşır.
/// </summary>
public class ValidationExceptionModel
{
    /// <summary>Hata alan adı.</summary>
    public string? Property { get; set; }
    /// <summary>Alan için hata mesajları.</summary>
    public IEnumerable<string>? Errors { get; set; }
}

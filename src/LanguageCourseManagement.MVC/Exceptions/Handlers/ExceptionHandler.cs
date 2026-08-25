using LanguageCourseManagement.Application.Exceptions;

namespace LanguageCourseManagement.MVC.Exceptions.Handlers;

/// <summary>
/// Exception türüne göre uygun işleyiciye yönlendiren soyut temel sınıf.
/// </summary>
public abstract class ExceptionHandler
{
    /// <summary>
    /// Fırlatılan exception'ı türüne göre eşleştirip uygun handler'a iletir.
    /// </summary>
    public Task HandleExceptionAsync(Exception exception)
    {
        // Önce spesifik exception türleri kontrol edilir; eşleşme olmazsa genel Exception yakalanarak 500 döndürülür
        if (exception is BusinessException businessException)
            return HandleException(businessException);

        if (exception is ValidationException validationException)
            return HandleException(validationException);

        if (exception is AuthorizationException authorizationException)
            return HandleException(authorizationException);

        if (exception is NotFoundException notFoundException)
            return HandleException(notFoundException);

        return HandleException(exception);
    }

    protected abstract Task HandleException(BusinessException businessException);
    protected abstract Task HandleException(ValidationException validationException);
    protected abstract Task HandleException(AuthorizationException authorizationException);
    protected abstract Task HandleException(NotFoundException notFoundException);
    protected abstract Task HandleException(Exception exception);
}

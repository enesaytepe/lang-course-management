namespace LanguageCourseManagement.Application.Results;

public enum ResultErrorType
{
    Validation,
    NotFound,
    Conflict
}

public class BaseResult<T>
{
    public bool IsSuccess { get; set; }
    public List<string> ErrorMessages { get; set; } = [];
    public T? Data { get; set; }
    public ResultErrorType ErrorType { get; set; } = ResultErrorType.Validation;
}

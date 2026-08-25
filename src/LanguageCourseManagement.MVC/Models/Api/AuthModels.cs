namespace LanguageCourseManagement.MVC.Models.Api;

public sealed class ApiLoginRequest
{
    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public sealed class AuthUserResponse
{
    public string UserName { get; set; } = string.Empty;

    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}

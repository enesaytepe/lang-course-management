namespace LanguageCourseManagement.Application.DTOs.Users;

public sealed class UserListResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = [];
    public bool IsActive { get; set; }
}

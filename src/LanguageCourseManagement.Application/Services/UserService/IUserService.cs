using LanguageCourseManagement.Application.DTOs.Users;

namespace LanguageCourseManagement.Application.Services.UserService;

/// <summary>
/// Kullanıcı yönetim işlemlerini tanımlar.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Tüm kullanıcıları listeler.
    /// </summary>
    Task<List<UserListResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// ID'ye göre kullanıcı getirir.
    /// </summary>
    Task<UserListResponse?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni kullanıcı oluşturur.
    /// </summary>
    Task<UserListResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut kullanıcının bilgilerini günceller.
    /// </summary>
    Task<UserListResponse> UpdateUserAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcıyı siler.
    /// </summary>
    Task DeleteUserAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının şifresini değiştirir.
    /// </summary>
    Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}

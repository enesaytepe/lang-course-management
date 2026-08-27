using LanguageCourseManagement.Application.DTOs.Users;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.UserService;
using LanguageCourseManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LanguageCourseManagement.Infrastructure.Identity;

public sealed class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<List<UserListResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .ToListAsync(cancellationToken);

        var result = new List<UserListResponse>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserListResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Email = user.Email,
                Roles = roles.ToList(),
                IsActive = !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow
            });
        }

        return result;
    }

    public async Task<UserListResponse?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserListResponse
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles.ToList(),
            IsActive = !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow
        };
    }

    public async Task<UserListResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        request.UserName = request.UserName.Trim();
        request.FullName = request.FullName.Trim();
        request.Email = request.Email.Trim();

        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser is not null)
            throw new BusinessException("Bu kullanıcı adı zaten kullanılıyor.");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existingEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingEmail is not null)
                throw new BusinessException("Bu e-posta adresi zaten kullanılıyor.");
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
            throw new BusinessException($"'{request.Role}' rolü mevcut değil.");

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            FullName = request.FullName,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BusinessException($"Kullanıcı oluşturulamadı: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new BusinessException($"Rol atanamadı: {errors}");
        }

        _logger.LogInformation("[UserService] Yeni kullanıcı oluşturuldu - {UserId}, {UserName}", user.Id, user.UserName);

        var roles = await _userManager.GetRolesAsync(user);
        return new UserListResponse
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles.ToList(),
            IsActive = true
        };
    }

    public async Task<UserListResponse> UpdateUserAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        request.FullName = request.FullName.Trim();
        request.Email = request.Email.Trim();

        if (!await _roleManager.RoleExistsAsync(request.Role))
            throw new BusinessException($"'{request.Role}' rolü mevcut değil.");

        user.FullName = request.FullName;
        user.Email = request.Email;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new BusinessException($"Kullanıcı güncellenemedi: {errors}");
        }

        // Roller güncelle
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.SequenceEqual(new[] { request.Role }))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new BusinessException($"Rol güncellenemedi: {errors}");
            }
        }

        _logger.LogInformation("[UserService] Kullanıcı güncellendi - {UserId}", id);

        var roles = await _userManager.GetRolesAsync(user);
        return new UserListResponse
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles.ToList(),
            IsActive = !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow
        };
    }

    public async Task DeleteUserAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BusinessException($"Kullanıcı silinemedi: {errors}");
        }

        _logger.LogInformation("[UserService] Kullanıcı silindi - {UserId}", id);
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (request.NewPassword != request.ConfirmPassword)
            throw new BusinessException("Yeni şifre ve onay şifresi eşleşmiyor.");

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BusinessException($"Şifre değiştirilemedi: {errors}");
        }

        _logger.LogInformation("[UserService] Şifre değiştirildi - {UserId}", userId);
    }
}

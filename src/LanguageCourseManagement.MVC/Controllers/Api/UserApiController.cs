using LanguageCourseManagement.Application.DTOs.Users;
using LanguageCourseManagement.Application.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Kullanıcı yönetimi API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = "SystemAdmin")]
public sealed class UserApiController : ControllerBase
{
    private readonly IUserService _userService;

    public UserApiController(IUserService userService) => _userService = userService;

    /// <summary>
    /// Tüm kullanıcıları listeler.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<List<UserListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserListResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetAllUsersAsync(cancellationToken));
    }

    /// <summary>
    /// ID'ye göre kullanıcı getirir.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType<UserListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserListResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
            return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Yeni kullanıcı oluşturur.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<UserListResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<UserListResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Mevcut kullanıcının bilgilerini günceller.
    /// </summary>
    [HttpPut("{id}")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<UserListResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserListResponse>> Update(string id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userService.UpdateUserAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Kullanıcıyı siler.
    /// </summary>
    [HttpDelete("{id}")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _userService.DeleteUserAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kullanıcının şifresini değiştirir.
    /// </summary>
    [HttpPost("{id}/change-password")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword(string id, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await _userService.ChangePasswordAsync(id, request, cancellationToken);
        return NoContent();
    }
}

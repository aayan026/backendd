using FurnitureShop.Application.Dtos.User;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/users")]
[Authorize]
public class UsersController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;

    public UsersController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.FindByIdAsync(UserId);
        if (user is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        return OkResponse(new UserMeDto
        {
            Id          = user.Id,
            Email       = user.Email ?? string.Empty,
            Name        = user.Name,
            Surname     = user.Surname,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserMeDto dto)
    {
        var user = await _userManager.FindByIdAsync(UserId);
        if (user is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        user.Name        = dto.Name;
        user.Surname     = dto.Surname;
        user.PhoneNumber = dto.PhoneNumber;
        await _userManager.UpdateAsync(user);

        return UpdatedResponse();
    }

    /// <summary>
    /// Şifrəni dəyişdir — cari şifrəni yoxlayır, yeni şifrəni qeyd edir
    /// POST /api/users/me/change-password
    /// Body: { currentPassword, newPassword }
    /// </summary>
    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(UserId);
        if (user is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        var isValid = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
        if (!isValid)
            throw new UnauthorizedException(ValidationMessages.Get(Lang, "InvalidCredentials"));

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code.ToLower())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => ValidationMessages.Get(Lang, e.Code)).ToList());
            throw new Application.Exceptions.ValidationException(errors);
        }

        return UpdatedResponse();
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMe()
    {
        var user = await _userManager.FindByIdAsync(UserId);
        if (user is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        await _userManager.DeleteAsync(user);
        return DeletedResponse();
    }
}

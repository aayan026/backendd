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
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.Name,
            Surname = user.Surname
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserMeDto dto)
    {
        var user = await _userManager.FindByIdAsync(UserId);
        if (user is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "UserNotFound"));

        user.Name = dto.Name;
        user.Surname = dto.Surname;
        user.PhoneNumber = dto.PhoneNumber;
        await _userManager.UpdateAsync(user);

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

using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FurnitureShop.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected BaseApiController() { }

    protected string Lang =>
        HttpContext?.Request.Headers["Accept-Language"]
            .FirstOrDefault()?.ToLower() is string l
            && new[] { "az", "ru", "en" }.Contains(l) ? l : "az";

    protected string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    protected string Msg(string key) =>
        ValidationMessages.Get(Lang, key);

    protected IActionResult OkResponse<T>(T? data) =>
        Ok(ApiResponse<T>.Ok(data, Msg("Success")));

    protected IActionResult CreatedResponse<T>(T? data) =>
        StatusCode(201, ApiResponse<T>.Created(data, Msg("Created")));

    protected IActionResult UpdatedResponse() =>
        Ok(ApiResponse<object>.Ok(null, Msg("Updated")));

    protected IActionResult DeletedResponse() =>
        Ok(ApiResponse<object>.Ok(null, Msg("Deleted")));
}

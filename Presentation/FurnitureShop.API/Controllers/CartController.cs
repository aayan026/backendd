using FurnitureShop.Application.Dtos.Cart;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/cart")]
[Authorize]
public class CartController : BaseApiController
{
    private readonly ICartService _service;

    public CartController(ICartService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
        => OkResponse(await _service.GetAsync(UserId));

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        await _service.AddItemAsync(UserId, dto);
        return CreatedResponse<object>(null);
    }

    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] UpdateQuantityDto dto)
    {
        await _service.UpdateQuantityAsync(UserId, cartItemId, dto.Quantity);
        return UpdatedResponse();
    }

    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        await _service.RemoveItemAsync(UserId, cartItemId);
        return DeletedResponse();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        await _service.ClearAsync(UserId);
        return DeletedResponse();
    }
}

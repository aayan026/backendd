using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/wishlist")]
[Authorize]
public class WishlistController : BaseApiController
{
    private readonly IWishlistService _service;

    public WishlistController(IWishlistService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
        => OkResponse(await _service.GetAsync(UserId));

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromQuery] int? productId, [FromQuery] int? collectionId)
    {
        await _service.AddItemAsync(UserId, productId, collectionId);
        return CreatedResponse<object>(null);
    }

    [HttpDelete("items/{wishlistItemId}")]
    public async Task<IActionResult> RemoveItem(int wishlistItemId)
    {
        await _service.RemoveItemAsync(UserId, wishlistItemId);
        return DeletedResponse();
    }

    [HttpGet("check")]
    public async Task<IActionResult> IsInWishlist([FromQuery] int? productId, [FromQuery] int? collectionId)
    {
        var result = await _service.IsInWishlistAsync(UserId, productId, collectionId);
        return OkResponse(new { isInWishlist = result });
    }

    [HttpGet("check/{productId}")]
    public async Task<IActionResult> IsProductInWishlist(int productId)
    {
        var result = await _service.IsInWishlistAsync(UserId, productId, null);
        return OkResponse(new { isInWishlist = result });
    }
}

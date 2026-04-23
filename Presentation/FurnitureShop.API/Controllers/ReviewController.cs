using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Review;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FurnitureShop.API.Controllers;

[Route("api/reviews")]
public class ReviewController : BaseApiController
{
    private readonly IReviewService _service;

    public ReviewController(IReviewService service)
    {
        _service = service;
    }

    [HttpGet("by-product/{productId:int}")]
    public async Task<IActionResult> GetByProduct(
        int productId,
        [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetByProductAsync(productId, pagination);
        return Ok(ApiResponse<List<ReviewDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedResponse(created);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto dto)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email)
                     ?? User.FindFirstValue("email")
                     ?? string.Empty;

        var updated = await _service.UpdateAsync(id, dto, userEmail);
        return OkResponse(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email)
                     ?? User.FindFirstValue("email")
                     ?? string.Empty;

        await _service.DeleteAsync(id, userEmail);
        return DeletedResponse();
    }
}
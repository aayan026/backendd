using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Review;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedResponse(created);
    }
}

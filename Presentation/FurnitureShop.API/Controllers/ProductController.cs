using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Product;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/products")]
public class ProductController : BaseApiController
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetAllAsync(pagination);
        return Ok(ApiResponse<List<ProductDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
        => OkResponse(await _service.GetFeaturedAsync());

    [HttpGet("by-category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId, [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetPagedAsync(categoryId, pagination);
        return Ok(ApiResponse<List<ProductDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [HttpGet("by-furniture-category/{categoryId}")]
    public async Task<IActionResult> GetByFurnitureCategory(int categoryId, [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetPagedAsync(categoryId, pagination);
        return Ok(ApiResponse<List<ProductDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [HttpGet("by-collection/{id}")]
    public async Task<IActionResult> GetByCollection(int id, [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetByCollectionAsync(id, pagination);
        return Ok(ApiResponse<List<ProductDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword, [FromQuery] PaginationParams pagination)
    {
        var result = await _service.SearchAsync(keyword, pagination);
        return Ok(ApiResponse<List<ProductDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [HttpGet("by-color")]
    public async Task<IActionResult> GetByColor([FromQuery] string color, [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetByColorAsync(color, pagination);
        return Ok(ApiResponse<List<ProductDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [HttpGet("price-range")]
    public async Task<IActionResult> GetByPriceRange(
        [FromQuery] decimal min,
        [FromQuery] decimal max,
        [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetByPriceRangeAsync(min, max, pagination);
        return Ok(ApiResponse<List<ProductDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetDetailAsync(id));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedResponse(new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (id != dto.Id)
            return BadRequest(ApiResponse<object>.ValidationError(
                new Dictionary<string, List<string>> { { "id", new List<string> { Msg("IdMismatch") } } },
                Msg("ValidationError")));

        await _service.UpdateAsync(dto);
        return UpdatedResponse();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return DeletedResponse();
    }
}

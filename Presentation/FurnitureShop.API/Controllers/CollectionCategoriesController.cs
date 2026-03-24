using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.CollectionCategory;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/collection-categories")]
public class CollectionCategoriesController : BaseApiController
{
    private readonly ICollectionCategoryService _service;

    public CollectionCategoriesController(ICollectionCategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => OkResponse(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCollectionCategoryDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedResponse(new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCollectionCategoryDto dto)
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

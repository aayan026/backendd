using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Collection;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/collections")]
public class CollectionController : BaseApiController
{
    private readonly ICollectionService _service;

    public CollectionController(ICollectionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => OkResponse(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpGet("by-category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
        => OkResponse(await _service.GetByCategoryAsync(categoryId));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCollectionDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedResponse(new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCollectionDto dto)
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

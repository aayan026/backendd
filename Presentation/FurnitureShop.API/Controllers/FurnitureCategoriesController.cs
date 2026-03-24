using FurnitureShop.Application.Dtos.FurnitureCategory;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/furniture-categories")]
public class FurnitureCategoriesController : BaseApiController
{
    private readonly IFurnitureCategoryService _service;

    public FurnitureCategoriesController(IFurnitureCategoryService service)
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
    public async Task<IActionResult> Create([FromBody] CreateFurnitureCategoryDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedResponse(new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateFurnitureCategoryDto dto)
    {
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

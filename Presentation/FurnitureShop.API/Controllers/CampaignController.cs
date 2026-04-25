using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Campaign;
using FurnitureShop.Application.Dtos.Product;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/campaigns")]
public class CampaignController : BaseApiController
{
    private readonly ICampaignService _service;

    public CampaignController(ICampaignService service)
    {
        _service = service;
    }

    /// <summary>Public: aktiv kampaniyalar (hero slider)</summary>
    [HttpGet]
    public async Task<IActionResult> GetActive()
        => OkResponse(await _service.GetActiveAsync());

    /// <summary>Public: kampaniyanın scope-una görə məhsullar</summary>
    [HttpGet("{id:int}/products")]
    public async Task<IActionResult> GetProducts(int id, [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetCampaignProductsAsync(id, pagination);
        return Ok(ApiResponse<List<ProductDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    /// <summary>Admin: bütün kampaniyalar (paginated)</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetAllAsync(pagination);
        return Ok(ApiResponse<List<CampaignDto>>.Ok(result.Items, result.Pagination, Msg("Success")));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedResponse(new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCampaignDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return UpdatedResponse();
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        await _service.ToggleAsync(id);
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
using FurnitureShop.Application.Dtos.Campaign;
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

    [HttpGet]
    public async Task<IActionResult> GetActive()
        => OkResponse(await _service.GetActiveAsync());

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
        => OkResponse(await _service.GetAllAsync());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedResponse(new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return DeletedResponse();
    }
}

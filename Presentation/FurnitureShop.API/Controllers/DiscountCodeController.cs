using FurnitureShop.Application.Dtos.DiscountCode;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/discount-codes")]
public class DiscountCodeController : BaseApiController
{
    private readonly IDiscountCodeService _service;

    public DiscountCodeController(IDiscountCodeService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateDiscountCodeDto dto)
        => OkResponse(await _service.ValidateAsync(dto));

    // ── Admin ──────────────────────────────────────────────────────────────

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => OkResponse(await _service.GetAllAsync());

    [Authorize(Roles = "Admin")]
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
        => OkResponse(await _service.GetActiveAsync());

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDiscountCodeDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedResponse(new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateAsync(id);
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

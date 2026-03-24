using FurnitureShop.Application.Dtos.Address;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/addresses")]
[Authorize]
public class AddressController : BaseApiController
{
    private readonly IAddressService _service;

    public AddressController(IAddressService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => OkResponse(await _service.GetAllAsync(UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(UserId, id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
        => CreatedResponse(await _service.CreateAsync(UserId, dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressDto dto)
    {
        await _service.UpdateAsync(UserId, id, dto);
        return UpdatedResponse();
    }

    [HttpPatch("{id}/set-default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        await _service.SetDefaultAsync(UserId, id);
        return UpdatedResponse();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(UserId, id);
        return DeletedResponse();
    }
}

using FurnitureShop.Application.Dtos.Contact;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;


[Route("api/contact")]
public class ContactController : BaseApiController
{
    private readonly IContactService _contact;

    public ContactController(IContactService contact)
    {
        _contact = contact;
    }


    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] ContactMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Ad mütləqdir." });

        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            return BadRequest(new { message = "Düzgün email daxil edin." });

        if (string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(new { message = "Mesaj mütləqdir." });

        await _contact.SendContactMessageAsync(dto);

        return Ok(new { message = "Mesajınız göndərildi. Ən qısa zamanda sizinlə əlaqə saxlayacağıq." });
    }
}

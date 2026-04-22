using FurnitureShop.Application.Dtos.Contact;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/contact")]
public class ContactController : BaseApiController
{
    private readonly IContactService _contact;
    private readonly ILanguageService _lang;

    public ContactController(IContactService contact, ILanguageService lang)
    {
        _contact = contact;
        _lang = lang;
    }

    /// <summary>
    /// Contact formu göndər.
    /// Bütün validasiya FluentValidation (ValidationFilter) tərəfindən həyata keçirilir —
    /// 3 dildə (az/en/ru) xəta mesajları avtomatik qaytarılır.
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] ContactMessageDto dto)
    {
        await _contact.SendContactMessageAsync(dto);
        var lang = _lang.GetCurrentLanguage();
        return Ok(new { message = ValidationMessages.Get(lang, "ContactSent") });
    }
}

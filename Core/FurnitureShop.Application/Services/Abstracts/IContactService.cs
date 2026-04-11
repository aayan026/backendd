using FurnitureShop.Application.Dtos.Contact;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IContactService
{
    /// <summary>
    /// Contact form məlumatlarını həm admin emailinə həm admin telefon nömrəsinə göndərir.
    /// </summary>
    Task SendContactMessageAsync(ContactMessageDto dto);
}

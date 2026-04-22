using FurnitureShop.Application.Dtos.Contact;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IContactService
{

    Task SendContactMessageAsync(ContactMessageDto dto);
}

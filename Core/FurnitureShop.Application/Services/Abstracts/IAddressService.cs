using FurnitureShop.Application.Dtos.Address;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IAddressService
{
    Task<IEnumerable<AddressDto>> GetAllAsync(string userId);
    Task<AddressDto> GetByIdAsync(string userId, int addressId);
    Task<AddressDto> CreateAsync(string userId, CreateAddressDto dto);
    Task UpdateAsync(string userId, int addressId, UpdateAddressDto dto);
    Task SetDefaultAsync(string userId, int addressId);
    Task DeleteAsync(string userId, int addressId);
}

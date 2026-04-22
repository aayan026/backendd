using FurnitureShop.Application.Dtos.DiscountCode;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IDiscountCodeService
{
    Task<DiscountCodeValidationResult> ValidateAsync(ValidateDiscountCodeDto dto);

    Task<IEnumerable<DiscountCodeDto>> GetAllAsync();
    Task<IEnumerable<DiscountCodeDto>> GetActiveAsync();
    Task<DiscountCodeDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateDiscountCodeDto dto);
    Task DeactivateAsync(int id);
    Task DeleteAsync(int id);
}

using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IDiscountCodeReadRepository : IGenericReadRepository<DiscountCode>
{
    // Kod ilə gətir (checkout-da istifadəçi daxil edir)
    Task<DiscountCode?> GetByCodeAsync(string code);

    // Bütün aktiv kodlar (admin üçün)
    Task<IEnumerable<DiscountCode>> GetActiveAsync();
//admin ucuns
    Task<IEnumerable<DiscountCode>> GetAllAsync();

}

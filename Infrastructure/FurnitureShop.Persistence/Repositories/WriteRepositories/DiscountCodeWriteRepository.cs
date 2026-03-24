using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;

namespace FurnitureShop.Persistence.Repositories.WriteRepositories;

public class DiscountCodeWriteRepository : GenericWriteRepository<DiscountCode>, IDiscountCodeWriteRepository
{
    public DiscountCodeWriteRepository(AppDbContext context) : base(context)
    {
    }
    public async Task RemoveAsync(DiscountCode entity)
    {
        _context.DiscountCodes.Remove(entity);
        await _context.SaveChangesAsync();
    }
}

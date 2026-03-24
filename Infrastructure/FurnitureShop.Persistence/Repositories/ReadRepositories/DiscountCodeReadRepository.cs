using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Enums;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.ReadRepositories;

public class DiscountCodeReadRepository : GenericReadRepository<DiscountCode>, IDiscountCodeReadRepository
{
    public DiscountCodeReadRepository(AppDbContext context) : base(context) { }

    public async Task<DiscountCode?> GetByCodeAsync(string code)
        => await Table
            .FirstOrDefaultAsync(x => x.Code == code);
    public async Task<IEnumerable<DiscountCode>> GetAllAsync()
    {
        return await _context.DiscountCodes.ToListAsync();
    }



    public async Task<IEnumerable<DiscountCode>> GetActiveAsync()
        => await Table
            .Where(x => x.Status == DiscountStatus.Active &&
                        (x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow) &&
                        (x.MaxUses == null || x.UsedCount < x.MaxUses))
            .ToListAsync();
}

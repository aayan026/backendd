using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.ReadRepositories;

public class WishlistReadRepository : GenericReadRepository<Wishlist>, IWishlistReadRepository
{
    public WishlistReadRepository(AppDbContext context) : base(context) { }

    public async Task<Wishlist?> GetByUserIdAsync(string userId)
        => await Table
            .Where(x => x.UserId == userId)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images.Where(img => img.IsPrimary))
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Translations)
            .Include(x => x.Items)
                .ThenInclude(i => i.Collection)
                    .ThenInclude(c => c!.Translations)
            .FirstOrDefaultAsync();
}

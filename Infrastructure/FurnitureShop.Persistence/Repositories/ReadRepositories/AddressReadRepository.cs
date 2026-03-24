using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.ReadRepositories;

public class AddressReadRepository : GenericReadRepository<Address>, IAddressReadRepository
{
    public AddressReadRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Address>> GetByUserIdAsync(string userId)
        => await Table
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ToListAsync();

    public async Task<Address?> GetDefaultAddressAsync(string userId)
        => await Table
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDefault);
}

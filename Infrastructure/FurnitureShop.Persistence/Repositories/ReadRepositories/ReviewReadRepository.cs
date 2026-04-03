using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.ReadRepositories;

public class ReviewReadRepository : GenericReadRepository<Review>, IReviewReadRepository
{
    public ReviewReadRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Review>> GetByProductAsync(int productId)
        => await Table
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<double> GetAverageRatingAsync(int productId)
    {
        var ratings = await Table
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .Select(r => r.Rating)
            .ToListAsync();

        return ratings.Any() ? ratings.Average() : 0;
    }

    public async Task<int> GetCountAsync(int productId)
        => await Table
            .AsNoTracking()
            .CountAsync(r => r.ProductId == productId && r.IsApproved);
}

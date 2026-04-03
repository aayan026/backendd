using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Application.Repsitories.ReadRepositories;

public interface IReviewReadRepository : IGenericReadRepository<Review>
{
    Task<IEnumerable<Review>> GetByProductAsync(int productId);
    Task<double>              GetAverageRatingAsync(int productId);
    Task<int>                 GetCountAsync(int productId);
}

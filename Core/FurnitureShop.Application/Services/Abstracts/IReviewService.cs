using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Review;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IReviewService
{
    Task<PagedList<ReviewDto>> GetByProductAsync(int productId, PaginationParams pagination);
    Task<ReviewDto> CreateAsync(CreateReviewDto dto);
    Task<ReviewDto> UpdateAsync(int id, UpdateReviewDto dto, string userEmail);
    Task DeleteAsync(int id, string userEmail);
}
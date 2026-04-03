using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Review;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Persistence.Services.Concretes;

public class ReviewService : IReviewService
{
    private readonly IReviewReadRepository  _readRepo;
    private readonly IReviewWriteRepository _writeRepo;
    private readonly IProductReadRepository _productReadRepo;

    public ReviewService(
        IReviewReadRepository  readRepo,
        IReviewWriteRepository writeRepo,
        IProductReadRepository productReadRepo)
    {
        _readRepo        = readRepo;
        _writeRepo       = writeRepo;
        _productReadRepo = productReadRepo;
    }

    public async Task<PagedList<ReviewDto>> GetByProductAsync(int productId, PaginationParams pagination)
    {
        var reviews = await _readRepo.GetByProductAsync(productId);
        var paged   = PagedList<Review>.Create(reviews, pagination.Page, pagination.PageSize);

        return new PagedList<ReviewDto>
        {
            Items = paged.Items.Select(r => new ReviewDto
            {
                Id         = r.Id,
                ProductId  = r.ProductId,
                AuthorName = r.AuthorName,
                Rating     = r.Rating,
                Comment    = r.Comment,
                CreatedAt  = r.CreatedAt
            }).ToList(),
            Pagination = paged.Pagination
        };
    }

    public async Task<ReviewDto> CreateAsync(CreateReviewDto dto)
    {
        // məhsulun mövcudluğunu yoxla
        var product = await _productReadRepo.GetByIdAsync(dto.ProductId);
        if (product is null)
            throw new NotFoundException("Məhsul tapılmadı.");

        if (dto.Rating < 1 || dto.Rating > 5)
            throw new Exception("Reytinq 1 ilə 5 arasında olmalıdır.");

        var review = new Review
        {
            ProductId   = dto.ProductId,
            AuthorName  = dto.AuthorName.Trim(),
            AuthorEmail = dto.AuthorEmail?.Trim(),
            Rating      = dto.Rating,
            Comment     = dto.Comment.Trim(),
            IsApproved  = true,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        await _writeRepo.AddAsync(review);
        await _writeRepo.SaveChangesAsync();

        return new ReviewDto
        {
            Id         = review.Id,
            ProductId  = review.ProductId,
            AuthorName = review.AuthorName,
            Rating     = review.Rating,
            Comment    = review.Comment,
            CreatedAt  = review.CreatedAt
        };
    }
}

using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Review;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Domain.Entities.Concretes;
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class ReviewService : IReviewService
{
    private readonly IReviewReadRepository  _readRepo;
    private readonly IReviewWriteRepository _writeRepo;
    private readonly IProductReadRepository _productReadRepo;
    private static readonly ILogger _log = Log.ForContext<ReviewService>();

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
        _log.Information("Məhsul rəyləri sorğusu — ProductId: {ProductId}", productId);
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
        _log.Information("Yeni rəy əlavə edilir — ProductId: {ProductId} Müəllif: {Author} Reytinq: {Rating}",
            dto.ProductId, dto.AuthorName, dto.Rating);

        var product = await _productReadRepo.GetByIdAsync(dto.ProductId);
        if (product is null)
        {
            _log.Warning("Rəy əlavə edilə bilmədi — Məhsul tapılmadı — ProductId: {ProductId}", dto.ProductId);
            throw new NotFoundException("Məhsul tapılmadı.");
        }

        if (dto.Rating < 1 || dto.Rating > 5)
        {
            _log.Warning("Rəy əlavə edilə bilmədi — Yanlış reytinq — Rating: {Rating}", dto.Rating);
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>>
                {
                    { "rating", new List<string> { "Reytinq 1 ilə 5 arasında olmalıdır." } }
                });
        }

        if (!string.IsNullOrWhiteSpace(dto.AuthorEmail))
        {
            var existing = await _readRepo.AnyAsync(r =>
                r.ProductId   == dto.ProductId &&
                r.AuthorEmail == dto.AuthorEmail.Trim().ToLower());

            if (existing)
            {
                _log.Warning("Dublikat rəy cəhdi — ProductId: {ProductId} Email: {Email}", dto.ProductId, dto.AuthorEmail);
                throw new Application.Exceptions.ValidationException(
                    new Dictionary<string, List<string>>
                    {
                        { "review", new List<string> { "Bu məhsula artıq rəy bildirmişsiniz." } }
                    });
            }
        }

        var review = new Review
        {
            ProductId   = dto.ProductId,
            AuthorName  = dto.AuthorName.Trim(),
            AuthorEmail = dto.AuthorEmail?.Trim().ToLower(),
            Rating      = dto.Rating,
            Comment     = dto.Comment.Trim(),
            IsApproved  = true,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        await _writeRepo.AddAsync(review);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Rəy uğurla əlavə edildi — ReviewId: {ReviewId} ProductId: {ProductId}", review.Id, review.ProductId);

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

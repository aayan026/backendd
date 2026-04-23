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
    private readonly IReviewReadRepository _readRepo;
    private readonly IReviewWriteRepository _writeRepo;
    private readonly IProductReadRepository _productReadRepo;
    private static readonly ILogger _log = Log.ForContext<ReviewService>();

    public ReviewService(
        IReviewReadRepository readRepo,
        IReviewWriteRepository writeRepo,
        IProductReadRepository productReadRepo)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _productReadRepo = productReadRepo;
    }

    public async Task<PagedList<ReviewDto>> GetByProductAsync(int productId, PaginationParams pagination)
    {
        _log.Information("Məhsul rəyləri sorğusu — ProductId: {ProductId}", productId);
        var reviews = await _readRepo.GetByProductAsync(productId);
        var paged = PagedList<Review>.Create(reviews, pagination.Page, pagination.PageSize);

        return new PagedList<ReviewDto>
        {
            Items = paged.Items.Select(ToDto).ToList(),
            Pagination = paged.Pagination
        };
    }

    public async Task<ReviewDto> CreateAsync(CreateReviewDto dto)
    {
        _log.Information("Yeni rəy əlavə edilir — ProductId: {ProductId} Müəllif: {Author} Reytinq: {Rating}",
            dto.ProductId, dto.AuthorName, dto.Rating);

        var product = await _productReadRepo.GetByIdAsync(dto.ProductId);
        if (product is null)
            throw new NotFoundException("Məhsul tapılmadı.");

        ValidateRating(dto.Rating);

        if (!string.IsNullOrWhiteSpace(dto.AuthorEmail))
        {
            var existing = await _readRepo.AnyAsync(r =>
                r.ProductId == dto.ProductId &&
                r.AuthorEmail == dto.AuthorEmail.Trim().ToLower());

            if (existing)
                throw new Application.Exceptions.ValidationException(
                    new Dictionary<string, List<string>>
                    {
                        { "review", new List<string> { "Bu məhsula artıq rəy bildirmişsiniz." } }
                    });
        }

        var review = new Review
        {
            ProductId = dto.ProductId,
            AuthorName = (dto.AuthorName?.Trim()) ?? "İstifadəçi",
            AuthorEmail = dto.AuthorEmail?.Trim().ToLower(),
            Rating = dto.Rating,
            Comment = dto.Comment.Trim(),
            IsApproved = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _writeRepo.AddAsync(review);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Rəy uğurla əlavə edildi — ReviewId: {ReviewId}", review.Id);
        return ToDto(review);
    }

    public async Task<ReviewDto> UpdateAsync(int id, UpdateReviewDto dto, string userEmail)
    {
        var review = await _readRepo.GetByIdAsync(id);
        if (review is null)
            throw new NotFoundException("Rəy tapılmadı.");

        if (!string.Equals(review.AuthorEmail, userEmail, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("Bu rəyi yalnız müəllif redaktə edə bilər.");

        ValidateRating(dto.Rating);

        review.Rating = dto.Rating;
        review.Comment = dto.Comment.Trim();
        review.UpdatedAt = DateTime.UtcNow;

        _writeRepo.Update(review);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Rəy yeniləndi — ReviewId: {ReviewId}", review.Id);
        return ToDto(review);
    }
    public async Task DeleteAsync(int id, string userEmail)
    {
        var review = await _readRepo.GetByIdAsync(id);
        if (review is null)
            throw new NotFoundException("Rəy tapılmadı.");

        if (!string.Equals(review.AuthorEmail, userEmail, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("Bu rəyi yalnız müəllif silə bilər.");

        _writeRepo.Delete(review);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Rəy silindi — ReviewId: {ReviewId}", review.Id);
    }

    private static ReviewDto ToDto(Review r) => new()
    {
        Id = r.Id,
        ProductId = r.ProductId,
        AuthorName = r.AuthorName,
        AuthorEmail = r.AuthorEmail,
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt
    };

    private static void ValidateRating(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>>
                {
                    { "rating", new List<string> { "Reytinq 1 ilə 5 arasında olmalıdır." } }
                });
    }
}
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Application.Common.Responses;

public class PagedList<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationMeta Pagination { get; set; } = null!;

    // IQueryable-dan avtomatik pagination — DB-də COUNT + SKIP + TAKE edir
    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<T>
        {
            Items = items,
            Pagination = new PaginationMeta(page, pageSize, totalCount)
        };
    }

    // In-memory list-dən pagination (artıq yüklənmiş data üçün)
    public static PagedList<T> Create(IEnumerable<T> source, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;

        var list = source.ToList();
        var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedList<T>
        {
            Items = items,
            Pagination = new PaginationMeta(page, pageSize, list.Count)
        };
    }
}

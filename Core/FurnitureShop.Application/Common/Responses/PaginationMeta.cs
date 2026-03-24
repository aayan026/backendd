namespace FurnitureShop.Application.Common.Responses;

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public PaginationMeta(int page, int pageSize, int totalCount)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}

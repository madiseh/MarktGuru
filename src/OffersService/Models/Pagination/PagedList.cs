using Microsoft.EntityFrameworkCore;

namespace OffersService.Models.Pagination;

public class PagedList<T>
{
    private PagedList(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;

    public static PagedList<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        return new PagedList<T>(items, page, pageSize, totalCount);
    }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<T>(items, page, pageSize, totalCount);
    }

    public PagedList<TResult> Map<TResult>(Func<T, TResult> map)
    {
        return new PagedList<TResult>(
            Items.Select(map).ToList(),
            Page,
            PageSize,
            TotalCount);
    }
}

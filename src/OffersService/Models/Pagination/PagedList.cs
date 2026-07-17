using Microsoft.EntityFrameworkCore;

namespace OffersService.Models.Pagination;

public class PagedList<T>
{
    private PagedList(IReadOnlyList<T> items, int page, int pageSize, int total)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        Total = total;
    }

    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int Total { get; }
    public bool HasNextPage => Page * PageSize < Total;
    public bool HasPreviousPage => Page > 1;

    public static PagedList<T> Create(IReadOnlyList<T> items, int page, int pageSize, int total)
    {
        return new PagedList<T>(items, page, pageSize, total);
    }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
    {
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<T>(items, page, pageSize, total);
    }

    public PagedList<TResult> Map<TResult>(Func<T, TResult> map)
    {
        return new PagedList<TResult>(
            Items.Select(map).ToList(),
            Page,
            PageSize,
            Total);
    }
}

namespace ModularMonolith.Shared.Abstractions.Common;

/// <summary>
/// Represents a paged list of items.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PagedList<T>
{
    public PagedList(IEnumerable<T> items, int page, int pageSize, int totalCount)
    {
        Items = items.ToList();
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public List<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

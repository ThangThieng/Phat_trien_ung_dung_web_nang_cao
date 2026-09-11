namespace CulinaryBlog.Application.Common.Models;

/// <summary>FR-RCP-001 / FR-SRCH-004 – kết quả phân trang offset.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;
}

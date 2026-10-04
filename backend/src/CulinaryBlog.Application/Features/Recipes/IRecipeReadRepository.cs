using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>Phân trang (FR-SRCH-004) + lọc (FR-SRCH-002) + sắp xếp (FR-SRCH-003) — đầu vào chung của mọi truy vấn danh sách.</summary>
public sealed record RecipeListCriteria(int Page, int PageSize, RecipeFilterSpec Filter, RecipeSort Sort);

/// <summary>
/// Read-side (CQRS Query) – implementation dùng EF Core projection, AsNoTracking, không N+1 (NFR-PERF-004).
/// Tên phương thức nói rõ phạm vi dữ liệu (MT-34): <c>Published…</c> là dữ liệu công khai, chỉ <c>Status == Published</c>, không
/// nhận danh tính người gọi làm tham số — không có cách nào vô tình trả bản nháp qua endpoint công khai. Dữ liệu riêng tư đi
/// qua <see cref="GetByAuthorPagedAsync"/> và <see cref="GetDetailByIdAsync"/>, chỉ gắn vào tiền tố <c>/recipes/mine</c> hoặc
/// response của command ghi đã được phân quyền.
/// </summary>
public interface IRecipeReadRepository
{
    /// <summary>FR-RCP-001, FR-CAT-002 — danh sách công khai.</summary>
    Task<PagedResult<RecipeSummaryDto>> GetPublishedPagedAsync(RecipeListCriteria criteria, CancellationToken cancellationToken);

    /// <summary>FR-RCP-002 — chi tiết công khai; công thức Draft/Archived/đã xóa trả <c>null</c> như slug không tồn tại.</summary>
    Task<RecipeDetailDto?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken);

    /// <summary>
    /// FR-SRCH-001 — tìm kiếm toàn văn trên công thức Published, xếp theo <c>ts_rank</c> giảm dần.
    /// <paramref name="tsQuery"/> là biểu thức <c>tsquery</c> đã được làm sạch (<see cref="SearchTermBuilder"/>).
    /// </summary>
    Task<PagedResult<RecipeSearchResultDto>> SearchPublishedAsync(
        string tsQuery,
        int page,
        int pageSize,
        RecipeFilterSpec filter,
        CancellationToken cancellationToken);

    /// <summary>MT-48 — mọi slug Published cho sitemap, không phân trang.</summary>
    Task<IReadOnlyList<RecipeSitemapEntryDto>> GetPublishedSitemapAsync(CancellationToken cancellationToken);

    /// <summary>FR-RCP-011 — công thức của MỘT tác giả ở mọi trạng thái (hoặc một trạng thái). Riêng tư: không bao giờ cache.</summary>
    Task<PagedResult<RecipeSummaryDto>> GetByAuthorPagedAsync(
        string authorId,
        RecipeStatus? status,
        RecipeListCriteria criteria,
        CancellationToken cancellationToken);

    /// <summary>
    /// Chi tiết theo id ở MỌI trạng thái — chỉ dùng sau khi người gọi đã được phân quyền trên công thức (response của
    /// command ghi, và <c>GET /recipes/mine/{id}</c>). Không bao giờ gắn vào endpoint công khai (MT-34).
    /// </summary>
    Task<RecipeDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken);
}

using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>Phân trang (FR-SRCH-004) + lọc (FR-SRCH-002) + sắp xếp (FR-SRCH-003) — đầu vào chung của mọi truy vấn danh sách.</summary>
public sealed record RecipeListCriteria(int Page, int PageSize, RecipeFilterSpec Filter, RecipeSort Sort);

/// <summary>
/// Read-side (CQRS Query) – implementation dùng EF Core projection, AsNoTracking, không N+1 (NFR-PERF-004).
/// Tên phương thức nói rõ phạm vi dữ liệu (MT-34): <c>Published…</c> là dữ liệu công khai, chỉ <c>Status == Published</c>, không
/// nhận danh tính người gọi làm tham số — không có cách nào vô tình trả bản nháp qua endpoint công khai. Dữ liệu ở mọi trạng
/// thái chỉ đi qua <see cref="GetDetailByIdAsync"/>, dùng cho response của command ghi đã được phân quyền.
/// </summary>
public interface IRecipeReadRepository
{
    /// <summary>FR-RCP-001, FR-CAT-002 — danh sách công khai.</summary>
    Task<PagedResult<RecipeSummaryDto>> GetPublishedPagedAsync(RecipeListCriteria criteria, CancellationToken cancellationToken);

    /// <summary>FR-RCP-002 — chi tiết công khai; công thức Draft/Archived/đã xóa trả <c>null</c> như slug không tồn tại.</summary>
    Task<RecipeDetailDto?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken);

    /// <summary>
    /// Chi tiết theo id ở MỌI trạng thái — chỉ dùng sau khi người gọi đã được phân quyền trên công thức (response của
    /// command ghi). Không bao giờ gắn vào endpoint công khai (MT-34).
    /// </summary>
    Task<RecipeDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken);
}

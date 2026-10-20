using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Domain.Entities;

/// <summary>
/// NGUỒN DUY NHẤT của máy trạng thái <see cref="RecipeStatus"/> (SRS §3.3 FR-RCP-005/006, MT-35 — retrofit D-15, Buổi 4):
///
/// | Từ               | Hành động  | Sang      |
/// |------------------|------------|-----------|
/// | Draft            | Publish    | Published |
/// | Published        | Unpublish  | Draft     |
/// | Draft, Published | Archive    | Archived  |
/// | Archived         | Unarchive  | Draft     |
///
/// Mọi cặp (trạng thái, hành động) KHÔNG có trong bảng — kể cả "publish một công thức đã Published" — là chuyển đổi không
/// hợp lệ → 409 RECIPE_INVALID_STATE_TRANSITION. Thêm trạng thái mới (ví dụ PendingReview) chỉ là thêm dòng; FE (Buổi 7)
/// dựng nút hành động từ chính bảng này.
/// </summary>
public static class RecipeStatusTransitions
{
    private static readonly Dictionary<(RecipeStatus From, RecipeAction Action), RecipeStatus> Table = new()
    {
        [(RecipeStatus.Draft, RecipeAction.Publish)] = RecipeStatus.Published,
        [(RecipeStatus.Published, RecipeAction.Unpublish)] = RecipeStatus.Draft,
        [(RecipeStatus.Draft, RecipeAction.Archive)] = RecipeStatus.Archived,
        [(RecipeStatus.Published, RecipeAction.Archive)] = RecipeStatus.Archived,
        [(RecipeStatus.Archived, RecipeAction.Unarchive)] = RecipeStatus.Draft,
    };

    /// <summary>Toàn bộ chuyển đổi hợp lệ — để kiểm thử và tài liệu hóa, không để sửa.</summary>
    public static IReadOnlyDictionary<(RecipeStatus From, RecipeAction Action), RecipeStatus> All => Table;

    public static bool TryGetTarget(RecipeStatus from, RecipeAction action, out RecipeStatus target) =>
        Table.TryGetValue((from, action), out target);
}

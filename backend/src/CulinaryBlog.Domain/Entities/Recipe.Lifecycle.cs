using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions.Recipes;

namespace CulinaryBlog.Domain.Entities;

/// <summary>
/// Vòng đời của aggregate Recipe (Buổi 4 — Dev 4): xuất bản ⇄ hủy xuất bản → lưu trữ → khôi phục, và xóa mềm.
/// Mọi chuyển trạng thái đi qua <see cref="RecipeStatusTransitions"/>; không method nào tự viết <c>if (Status == …)</c>.
/// </summary>
public partial class Recipe
{
    /// <summary>
    /// FR-RCP-005: Draft → Published. Kiểm tra chuyển đổi TRƯỚC (Archived → 409 dù có đủ nội dung hay không), rồi mới
    /// kiểm tra đủ ≥ 1 bước và ≥ 1 nguyên liệu (400). <see cref="PublishedAt"/> chỉ gán ở lần xuất bản ĐẦU TIÊN
    /// (<c>??=</c>) — hủy xuất bản rồi xuất bản lại không đổi ngày xuất bản gốc (JSON-LD datePublished, MT-23).
    /// ⚠️ Công thức phải được nạp kèm Steps + Ingredients (GetByIdWithDetailsAsync), nếu không collection rỗng và công
    /// thức đủ điều kiện vẫn bị từ chối.
    /// </summary>
    public void Publish(DateTime utcNow)
    {
        var target = ResolveTransition(RecipeAction.Publish);

        if (_steps.Count == 0 || _ingredients.Count == 0)
        {
            throw new RecipePublishIncompleteException(_steps.Count == 0, _ingredients.Count == 0);
        }

        Status = target;
        PublishedAt ??= utcNow;
    }

    /// <summary>FR-RCP-005: Published → Draft. Giữ nguyên <see cref="PublishedAt"/>.</summary>
    public void Unpublish() => Status = ResolveTransition(RecipeAction.Unpublish);

    /// <summary>FR-RCP-006: Draft hoặc Published → Archived. Giữ nguyên <see cref="PublishedAt"/>.</summary>
    public void Archive() => Status = ResolveTransition(RecipeAction.Archive);

    /// <summary>FR-RCP-006 (MT-35): Archived → Draft — buộc tác giả rà soát lại nội dung trước khi công khai lần nữa.</summary>
    public void Unarchive() => Status = ResolveTransition(RecipeAction.Unarchive);

    /// <summary>
    /// FR-RCP-007: xóa MỀM — chỉ gán <c>IsDeleted = true</c>, Status giữ nguyên. Ba hệ quả có chủ đích (MT-05):
    /// (1) bước/nguyên liệu/ảnh KHÔNG bị đụng tới — chúng vô hình cùng công thức qua Global Query Filter;
    /// (2) tệp ảnh trên MinIO KHÔNG bị xóa — khôi phục trong 30 ngày (NFR-REL-003) mới còn ý nghĩa;
    /// (3) slug được giải phóng nhờ partial unique index (D-3). Dọn vĩnh viễn là việc của PermanentPurgeJob.
    /// </summary>
    public void SoftDelete() => IsDeleted = true;

    private RecipeStatus ResolveTransition(RecipeAction action) =>
        RecipeStatusTransitions.TryGetTarget(Status, action, out var target)
            ? target
            : throw new InvalidRecipeStatusException(Status, action.ToString());
}

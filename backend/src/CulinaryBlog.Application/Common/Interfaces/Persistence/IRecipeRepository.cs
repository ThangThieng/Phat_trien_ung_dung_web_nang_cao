using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Common.Interfaces.Persistence;

/// <summary>
/// Repository GHI của aggregate Recipe (Buổi 3 — Dev 2). Recipe là aggregate root (SRS §3.3): bước, nguyên liệu, ảnh chỉ
/// được sửa qua <see cref="Recipe"/>, nên không có IRecipeStepRepository hay IRecipeImageRepository.
/// Hai mức nạp dữ liệu thay vì mở IQueryable: nếu mỗi handler tự Include, lỗi "Publish() khi chưa Include(Steps) nên
/// recipe đủ điều kiện vẫn bị từ chối" sẽ lặp lại ở mọi nơi. Phía đọc/hiển thị nằm ở <c>IRecipeReadRepository</c>.
/// </summary>
public interface IRecipeRepository : IRepository<Recipe>
{
    /// <summary>Nạp đủ Steps + Ingredients + Images (split query; Nutrition là owned nên tự đi kèm). Dùng cho nội dung, cập nhật, vòng đời.</summary>
    Task<Recipe?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Chỉ nạp Images — cho các command ảnh chạy thường xuyên, không kéo bước/nguyên liệu không dùng tới.</summary>
    Task<Recipe?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Slug đã có công thức CHƯA xóa nào giữ chưa (IDX_Recipe_Slug là partial unique — D-3).</summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// FR-RCP-004 (Buổi 4): đặt RowVersion GỐC mà client đang cầm (If-Match / body) làm giá trị so sánh của câu UPDATE —
    /// lệch với DB thì SaveChanges ném DbUpdateConcurrencyException → RecipeConcurrencyException (409). Handler không chạm DbContext.
    /// </summary>
    void SetOriginalRowVersion(Recipe recipe, byte[] rowVersion);
}

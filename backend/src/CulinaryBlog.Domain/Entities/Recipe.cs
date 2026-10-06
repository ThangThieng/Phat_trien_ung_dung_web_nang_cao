using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Recipes;

namespace CulinaryBlog.Domain.Entities;

/// <summary>
/// SRS §7.2 – Aggregate Root: chứa Steps, Ingredients, Images và Owned Entity Nutrition.
/// <c>partial</c> theo trách nhiệm (Buổi 4): file này giữ NỘI DUNG (Dev 2); vòng đời trạng thái và xóa mềm nằm ở
/// <c>Recipe.Lifecycle.cs</c> (Dev 4) — cùng một aggregate, mỗi người sửa một file.
/// </summary>
public partial class Recipe : BaseEntity
{
    private readonly List<RecipeStep> _steps = [];
    private readonly List<RecipeIngredient> _ingredients = [];
    private readonly List<RecipeImage> _images = [];

    private Recipe()
    {
    }

    public string Title { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string? Instructions { get; private set; }

    public int PrepTimeMinutes { get; private set; }

    public int CookTimeMinutes { get; private set; }

    public int Servings { get; private set; }

    public RecipeDifficulty Difficulty { get; private set; } = RecipeDifficulty.Easy;

    public RecipeStatus Status { get; private set; } = RecipeStatus.Draft;

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public string AuthorId { get; private set; } = string.Empty;

    public DateTime? PublishedAt { get; private set; }

    public RecipeNutrition? Nutrition { get; private set; }

    public IReadOnlyCollection<RecipeStep> Steps => _steps.AsReadOnly();

    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients.AsReadOnly();

    /// <summary>
    /// Ảnh chưa bị xóa. Ảnh xóa mềm (FR-RCP-008 bước 14) vẫn nằm trong backing field cho tới lần lưu kế tiếp: gỡ nó khỏi
    /// collection thì EF coi là bản ghi mồ côi và XÓA CỨNG — trái yêu cầu IsDeleted = true của SRS.
    /// </summary>
    public IReadOnlyCollection<RecipeImage> Images => _images.Where(i => !i.IsDeleted).ToList().AsReadOnly();

    public static Recipe Create(
        string title,
        string slug,
        string description,
        Guid categoryId,
        string authorId,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int servings,
        RecipeDifficulty difficulty,
        string? instructions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(authorId);

        if (prepTimeMinutes <= 0 || cookTimeMinutes < 0 || servings <= 0)
        {
            throw new BusinessRuleViolationException(ErrorCodes.ValidationError, "PrepTime và Servings phải > 0, CookTime phải >= 0.");
        }

        return new Recipe
        {
            Title = title.Trim(),
            Slug = slug,
            Description = description,
            Instructions = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim(),
            CategoryId = categoryId,
            AuthorId = authorId,
            PrepTimeMinutes = prepTimeMinutes,
            CookTimeMinutes = cookTimeMinutes,
            Servings = servings,
            Difficulty = difficulty,
            Status = RecipeStatus.Draft,
        };
    }

    /// <summary>
    /// FR-RCP-004 / NFR-SEO-004 (MT-27): slug bị KHÓA VĨNH VIỄN từ lần xuất bản đầu tiên (<see cref="PublishedAt"/> đã gán) —
    /// link đã chia sẻ và đã được lập chỉ mục không bao giờ chết; hệ thống không có bảng lịch sử slug hay 301.
    /// Công thức Archived cũng giữ nguyên slug. Chỉ bản nháp CHƯA TỪNG xuất bản mới đổi slug theo tiêu đề.
    /// </summary>
    public bool IsSlugLocked => PublishedAt is not null || Status != RecipeStatus.Draft;

    /// <summary>FR-RCP-004: cập nhật thông tin cơ bản (người gọi đã gộp các trường PATCH-style với giá trị hiện tại).</summary>
    public void Update(
        string title,
        string description,
        Guid categoryId,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int servings,
        RecipeDifficulty difficulty,
        string? instructions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (prepTimeMinutes <= 0 || cookTimeMinutes < 0 || servings <= 0)
        {
            throw new BusinessRuleViolationException(ErrorCodes.ValidationError, "PrepTime và Servings phải > 0, CookTime phải >= 0.");
        }

        Title = title.Trim();
        Description = description.Trim();
        CategoryId = categoryId;
        PrepTimeMinutes = prepTimeMinutes;
        CookTimeMinutes = cookTimeMinutes;
        Servings = servings;
        Difficulty = difficulty;
        Instructions = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim();
    }

    /// <summary>Đổi slug — chỉ hợp lệ khi <see cref="IsSlugLocked"/> = false (handler kiểm tra trước; đây là chốt chặn cuối).</summary>
    public void ChangeSlug(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        if (IsSlugLocked)
        {
            throw new InvalidOperationException("Slug của công thức đã từng xuất bản bị khóa vĩnh viễn.");
        }

        Slug = slug;
    }

    // ---------------- Bước nấu (FR-RCP-010) ----------------
    public RecipeStep GetStep(Guid stepId) =>
        _steps.SingleOrDefault(s => s.Id == stepId) ?? throw new RecipeStepNotFoundException(Id, stepId);

    /// <summary>Server gán <c>StepNumber = Max + 1</c> (1 nếu chưa có bước) — client không bao giờ gửi số thứ tự (MT-03).</summary>
    public RecipeStep AddStep(string title, string description, int? timerMinutes = null, string? imageUrl = null)
    {
        var nextNumber = _steps.Count == 0 ? 1 : _steps.Max(s => s.StepNumber) + 1;
        var step = RecipeStep.Create(Id, nextNumber, title, description, timerMinutes, imageUrl);
        _steps.Add(step);
        return step;
    }

    public RecipeStep UpdateStep(Guid stepId, string title, string description, int? timerMinutes, string? imageUrl)
    {
        var step = GetStep(stepId);
        step.Update(title, description, timerMinutes, imageUrl);
        return step;
    }

    /// <summary>
    /// Xóa bước rồi đánh số lại 1..N theo thứ tự hiện tại. Xóa CỨNG (gỡ khỏi aggregate → EF xóa bản ghi mồ côi): bước là
    /// một phần nội dung của công thức, khả năng khôi phục áp dụng cho CẢ công thức (FR-RCP-007) — và một bước xóa mềm vẫn
    /// giữ StepNumber trong UQ_RecipeStep_Recipe_StepNumber, chặn việc đánh số lại.
    /// Mọi lệnh gán lại số nằm trong MỘT SaveChanges; constraint DEFERRABLE (D-16) cho phép trạng thái trung gian trùng số.
    /// </summary>
    public void RemoveStep(Guid stepId)
    {
        var step = GetStep(stepId);
        _steps.Remove(step);
        RenumberSteps(_steps.OrderBy(s => s.StepNumber).ToList());
    }

    /// <summary>
    /// FR-RCP-010 bước 9–11: nhận Ý ĐỊNH ("thứ tự mới là mảng id này"), không nhận giá trị cột. Tập id phải KHỚP CHÍNH XÁC
    /// tập bước hiện có — thiếu, thừa hoặc trùng → 400 VALIDATION_ERROR nêu rõ id sai.
    /// </summary>
    public IReadOnlyList<RecipeStep> ReorderSteps(IReadOnlyList<Guid> stepIds)
    {
        ArgumentNullException.ThrowIfNull(stepIds);

        var duplicates = stepIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        var existing = _steps.Select(s => s.Id).ToHashSet();
        var unknown = stepIds.Where(id => !existing.Contains(id)).Distinct().ToList();
        var missing = existing.Where(id => !stepIds.Contains(id)).ToList();

        if (duplicates.Count > 0 || unknown.Count > 0 || missing.Count > 0)
        {
            throw new BusinessRuleViolationException(ErrorCodes.ValidationError, DescribeMismatch(duplicates, unknown, missing));
        }

        var ordered = stepIds.Select(id => _steps.Single(s => s.Id == id)).ToList();
        RenumberSteps(ordered);
        return ordered;
    }

    // ---------------- Nguyên liệu (FR-RCP-009) ----------------
    public RecipeIngredient GetIngredient(Guid ingredientId) =>
        _ingredients.SingleOrDefault(i => i.Id == ingredientId) ?? throw new RecipeIngredientNotFoundException(Id, ingredientId);

    /// <summary>Thêm nguyên liệu; <paramref name="orderIndex"/> mặc định là cuối danh sách. Bất biến ở <see cref="IngredientDetails"/>.</summary>
    public RecipeIngredient AddIngredient(IngredientDetails details, int? orderIndex = null)
    {
        ArgumentNullException.ThrowIfNull(details);

        var index = orderIndex ?? (_ingredients.Count == 0 ? 0 : _ingredients.Max(i => i.OrderIndex) + 1);
        var ingredient = RecipeIngredient.Create(Id, details, index);
        _ingredients.Add(ingredient);
        return ingredient;
    }

    public RecipeIngredient UpdateIngredient(Guid ingredientId, IngredientDetails details, int? orderIndex)
    {
        ArgumentNullException.ThrowIfNull(details);

        var ingredient = GetIngredient(ingredientId);
        ingredient.Update(details, orderIndex);
        return ingredient;
    }

    /// <summary>Xóa cứng nguyên liệu (gỡ khỏi aggregate) — cùng lý do với <see cref="RemoveStep"/>.</summary>
    public void RemoveIngredient(Guid ingredientId) => _ingredients.Remove(GetIngredient(ingredientId));

    public RecipeImage GetImage(Guid imageId) =>
        _images.SingleOrDefault(i => i.Id == imageId && !i.IsDeleted) ?? throw new RecipeImageNotFoundException(Id, imageId);

    /// <summary>
    /// FR-RCP-008 bước 6: ảnh đầu tiên tự động là ảnh chính; <paramref name="isPrimary"/> = true hạ mọi ảnh khác xuống
    /// (bất biến "chỉ 1 ảnh chính"). <paramref name="orderIndex"/> mặc định là cuối danh sách.
    /// </summary>
    public RecipeImage AddImage(string originalUrl, string? altText, bool? isPrimary = null, int? orderIndex = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalUrl);

        var active = Images;
        var primary = isPrimary ?? active.Count == 0;
        if (primary)
        {
            DemoteAllImages();
        }

        var image = RecipeImage.Create(Id, originalUrl, altText, primary, orderIndex ?? active.Count);
        _images.Add(image);
        return image;
    }

    /// <summary>PATCH metadata ảnh (FR-RCP-008 bước 9–11). Đổi ảnh chính đi qua <see cref="SetPrimaryImage"/>.</summary>
    public RecipeImage UpdateImageMetadata(Guid imageId, string? altText, int? orderIndex)
    {
        var image = GetImage(imageId);
        image.UpdateMetadata(altText, orderIndex);
        return image;
    }

    /// <summary>
    /// Hạ mọi ảnh chính. ⚠️ <c>IDX_RecipeImage_Primary</c> là partial unique index — PostgreSQL không cho index là
    /// DEFERRABLE và EF không bảo đảm thứ tự hai lệnh UPDATE trong một batch, nên handler phải LƯU bước hạ này trước rồi
    /// mới nâng ảnh mới (hai SaveChanges trong một transaction).
    /// </summary>
    /// <returns><c>true</c> nếu trước đó có ảnh chính.</returns>
    public bool DemoteAllImages()
    {
        var hadPrimary = false;
        foreach (var image in _images.Where(i => i.IsPrimary))
        {
            image.SetPrimary(false);
            hadPrimary = true;
        }

        return hadPrimary;
    }

    /// <summary>Đặt <paramref name="imageId"/> làm ảnh chính, hạ mọi ảnh khác (quy tắc 1 của SRS §8.4).</summary>
    public RecipeImage SetPrimaryImage(Guid imageId)
    {
        var image = GetImage(imageId);
        DemoteAllImages();
        image.SetPrimary(true);
        return image;
    }

    /// <summary>Xóa mềm ảnh (FR-RCP-008 bước 14). Chọn ảnh chính thay thế ở <see cref="PromoteFallbackPrimaryImage"/> sau khi đã lưu.</summary>
    public RecipeImage RemoveImage(Guid imageId)
    {
        var image = GetImage(imageId);
        image.SoftDelete();
        return image;
    }

    /// <summary>
    /// Quy tắc 2 của SRS §8.4: không còn ảnh chính mà vẫn còn ảnh → ảnh có OrderIndex nhỏ nhất (hòa thì CreatedAt sớm
    /// nhất) lên làm ảnh chính. Tiêu chí xác định rõ ràng để không phụ thuộc thứ tự trả về của DB.
    /// </summary>
    public RecipeImage? PromoteFallbackPrimaryImage()
    {
        var active = _images.Where(i => !i.IsDeleted).ToList();
        if (active.Count == 0 || active.Exists(i => i.IsPrimary))
        {
            return null;
        }

        var next = active.OrderBy(i => i.OrderIndex).ThenBy(i => i.CreatedAt).First();
        next.SetPrimary(true);
        return next;
    }

    public void SetNutrition(RecipeNutrition? nutrition) => Nutrition = nutrition;

    private static void RenumberSteps(List<RecipeStep> ordered)
    {
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].Renumber(i + 1);
        }
    }

    private static string DescribeMismatch(List<Guid> duplicates, List<Guid> unknown, List<Guid> missing)
    {
        var parts = new List<string>();
        if (duplicates.Count > 0)
        {
            parts.Add("trùng: " + string.Join(", ", duplicates));
        }

        if (unknown.Count > 0)
        {
            parts.Add("không thuộc công thức: " + string.Join(", ", unknown));
        }

        if (missing.Count > 0)
        {
            parts.Add("thiếu: " + string.Join(", ", missing));
        }

        return "stepIds phải chứa đúng và đủ id các bước hiện có của công thức (" + string.Join("; ", parts) + ").";
    }
}

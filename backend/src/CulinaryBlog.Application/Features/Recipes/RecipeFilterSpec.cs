using System.Globalization;
using CulinaryBlog.Domain.Enums;
using FluentValidation;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// FR-SRCH-002 — MỘT specification lọc dùng chung cho mọi truy vấn danh sách công thức (hiện: <c>GetRecipesQuery</c>,
/// <c>GetCategoryBySlugQuery</c>; phần API còn lại của Buổi 4 dùng tiếp). Các tiêu chí kết hợp bằng AND; tiêu chí để trống thì bỏ qua.
/// Không chứa điều kiện trạng thái: endpoint công khai cố định <c>Status == Published</c> ở repository (MT-34).
/// </summary>
public sealed record RecipeFilterSpec(
    Guid? CategoryId = null,
    RecipeDifficulty? Difficulty = null,
    int? MaxCookTime = null,
    int? MaxPrepTime = null,
    int? MinServings = null)
{
    /// <summary>Chuỗi chuẩn hóa (thứ tự cố định, văn hóa bất biến) — một phần đầu vào của <c>{queryHash}</c> trong khóa cache.</summary>
    public string ToCacheSegment() => string.Create(
        CultureInfo.InvariantCulture,
        $"categoryId={CategoryId}&difficulty={Difficulty}&maxCookTime={MaxCookTime}&maxPrepTime={MaxPrepTime}&minServings={MinServings}");
}

/// <summary>
/// Quy tắc validation dùng chung cho phân trang (FR-SRCH-004), sắp xếp (FR-SRCH-003) và bộ lọc (FR-SRCH-002). Mọi giá trị
/// ngoài whitelist → 400 <c>VALIDATION_ERROR</c>, không im lặng bỏ qua (CONS-008: validation ở FluentValidation, không ở endpoint).
/// </summary>
public static class RecipeQueryRules
{
    public const int DefaultPageSize = 12;

    public const int MaxPageSize = 50;

    public static IRuleBuilderOptions<T, int> ValidPage<T>(this IRuleBuilder<T, int> rule) =>
        rule.GreaterThanOrEqualTo(1).WithMessage("page phải >= 1.");

    public static IRuleBuilderOptions<T, int> ValidPageSize<T>(this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(1, MaxPageSize).WithMessage("pageSize phải trong khoảng 1–50.");

    public static IRuleBuilderOptions<T, string?> ValidSortBy<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(SortMapper.IsValidSortBy).WithMessage(SortMapper.SortByMessage);

    public static IRuleBuilderOptions<T, string?> ValidSortOrder<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(SortMapper.IsValidSortOrder).WithMessage(SortMapper.SortOrderMessage);

    public static IRuleBuilderOptions<T, RecipeDifficulty?> ValidDifficulty<T>(this IRuleBuilder<T, RecipeDifficulty?> rule) =>
        rule.IsInEnum().WithMessage("difficulty chỉ chấp nhận: Easy, Medium, Hard, Expert.");

    public static IRuleBuilderOptions<T, int?> NonNegativeMinutes<T>(this IRuleBuilder<T, int?> rule) =>
        rule.GreaterThanOrEqualTo(0).WithMessage("Số phút phải >= 0.");

    public static IRuleBuilderOptions<T, int?> PositiveServings<T>(this IRuleBuilder<T, int?> rule) =>
        rule.GreaterThanOrEqualTo(1).WithMessage("minServings phải >= 1.");
}

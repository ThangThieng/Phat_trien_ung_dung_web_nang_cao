using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using FluentValidation;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// Nguyên liệu gửi lên (SRS §8.6): <c>{ name, quantity?, quantityText?, unit?, notes?, orderIndex? }</c>. Dùng CHUNG cho
/// <c>POST /recipes/{id}/ingredients</c> và mảng inline <c>ingredients?</c> của <c>POST /recipes</c> — một validator, một
/// bất biến Domain, không có bản sao thứ hai phải giữ đồng bộ.
/// </summary>
public sealed record IngredientInput(string? Name, decimal? Quantity, string? QuantityText, string? Unit, string? Notes, int? OrderIndex = null)
    : IIngredientInputFields
{
    public IngredientDetails ToDetails() => new(Name ?? string.Empty, Quantity, QuantityText, Unit, Notes);
}

/// <summary>Bước gửi lên (SRS §8.5): <c>{ title, description, timerMinutes?, imageUrl? }</c> — KHÔNG có stepNumber (MT-03).</summary>
public sealed record StepInput(string? Title, string? Description, int? TimerMinutes, string? ImageUrl) : IStepFields;

/// <summary>Giới hạn theo SRS §7.9 (validator và cột DB bằng nhau — MT-37).</summary>
public static class RecipeContentLimits
{
    public const int IngredientNameMax = 200;
    public const int QuantityTextMax = 50;
    public const int UnitMax = 50;
    public const int NotesMax = 500;
    public const int StepTitleMax = 200;
    public const int StepDescriptionMax = 2000;
    public const int ImageUrlMax = 500;

    /// <summary>numeric(10,3).</summary>
    public const decimal QuantityMax = 9_999_999.999m;

    /// <summary>Chặn body khổng lồ ở mảng inline của POST /recipes (một công thức thật hiếm khi quá vài chục dòng).</summary>
    public const int InlineItemsMax = 100;
}

/// <summary>
/// Quy tắc ĐỊNH DẠNG của nguyên liệu (400 VALIDATION_ERROR). Quy tắc NGHIỆP VỤ "không rỗng cả quantity, quantityText lẫn
/// unit" cố ý KHÔNG ở đây: nó thuộc Domain (IngredientDetails) và có mã riêng INGREDIENT_QUANTITY_REQUIRED (FR-RCP-009 A4).
/// </summary>
public sealed class IngredientInputValidator : IngredientFieldsValidator<IngredientInput>;

/// <summary>
/// Bộ luật của MỘT nguyên liệu thêm mới, viết một lần cho mọi hình dạng mang đủ trường: phần tử mảng inline của
/// POST /recipes (<see cref="IngredientInput"/>) và body phẳng của POST /recipes/{id}/ingredients (AddIngredientCommand).
/// Body phẳng thì lỗi nằm đúng khóa "name", "quantity"… — validator lồng (SetValidator) sẽ sinh "ingredient.Name",
/// Frontend không gắn được vào ô (D-11).
/// </summary>
public abstract class IngredientFieldsValidator<T> : AbstractValidator<T>
    where T : IIngredientInputFields
{
    protected IngredientFieldsValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên nguyên liệu không được để trống.")
            .MaximumLength(RecipeContentLimits.IngredientNameMax)
                .WithMessage($"Tên nguyên liệu tối đa {RecipeContentLimits.IngredientNameMax} ký tự.");

        IngredientRules.Apply(this);
    }
}

internal static class IngredientRules
{
    /// <summary>Các trường định lượng/ghi chú — dùng chung cho thêm mới và cập nhật.</summary>
    public static void Apply<T>(AbstractValidator<T> validator)
        where T : IIngredientFields
    {
        validator.RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.")
            .LessThanOrEqualTo(RecipeContentLimits.QuantityMax).WithMessage("Số lượng quá lớn.")
            .When(x => x.Quantity.HasValue);
        validator.RuleFor(x => x.QuantityText)
            .MaximumLength(RecipeContentLimits.QuantityTextMax)
            .WithMessage($"Định lượng dạng chữ tối đa {RecipeContentLimits.QuantityTextMax} ký tự.");
        validator.RuleFor(x => x.Unit)
            .MaximumLength(RecipeContentLimits.UnitMax).WithMessage($"Đơn vị tối đa {RecipeContentLimits.UnitMax} ký tự.");
        validator.RuleFor(x => x.Notes)
            .MaximumLength(RecipeContentLimits.NotesMax).WithMessage($"Ghi chú tối đa {RecipeContentLimits.NotesMax} ký tự.");
        validator.RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("orderIndex phải >= 0.")
            .When(x => x.OrderIndex.HasValue);
    }
}

/// <summary>Trường chung của nguyên liệu để <see cref="IngredientRules"/> áp một bộ luật cho cả thêm mới lẫn cập nhật.</summary>
public interface IIngredientFields
{
    decimal? Quantity { get; }

    string? QuantityText { get; }

    string? Unit { get; }

    string? Notes { get; }

    int? OrderIndex { get; }
}

/// <summary>Nguyên liệu thêm mới: thêm <see cref="Name"/> bắt buộc vào các trường chung.</summary>
public interface IIngredientInputFields : IIngredientFields
{
    string? Name { get; }
}

public sealed class StepInputValidator(IFileStorageService storage) : StepFieldsValidator<StepInput>(storage);

/// <summary>
/// Bộ luật của MỘT bước thêm mới — dùng chung cho phần tử mảng inline của POST /recipes (<see cref="StepInput"/>) và body
/// phẳng của POST /recipes/{id}/steps (AddStepCommand), để lỗi nằm đúng khóa "title"/"description" (D-11).
/// </summary>
public abstract class StepFieldsValidator<T> : AbstractValidator<T>
    where T : IStepFields
{
    protected StepFieldsValidator(IFileStorageService storage)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề bước không được để trống.")
            .MaximumLength(RecipeContentLimits.StepTitleMax).WithMessage($"Tiêu đề bước tối đa {RecipeContentLimits.StepTitleMax} ký tự.");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Mô tả bước không được để trống.")
            .MaximumLength(RecipeContentLimits.StepDescriptionMax)
                .WithMessage($"Mô tả bước tối đa {RecipeContentLimits.StepDescriptionMax} ký tự.");
        StepRules.Apply(this, storage);
    }
}

internal static class StepRules
{
    public static void Apply<T>(AbstractValidator<T> validator, IFileStorageService storage)
        where T : IStepOptionalFields
    {
        validator.RuleFor(x => x.TimerMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("timerMinutes phải >= 0.")
            .When(x => x.TimerMinutes.HasValue);

        // Ảnh minh họa bước: tệp đã tải lên qua POST /files/upload (SRS §8.8) — không nhận ảnh domain ngoài.
        validator.RuleFor(x => x.ImageUrl!)
            .MaximumLength(RecipeContentLimits.ImageUrlMax).WithMessage($"Đường dẫn ảnh tối đa {RecipeContentLimits.ImageUrlMax} ký tự.")
            .Must(storage.IsStoredFileUrl).WithMessage("Ảnh của bước phải là tệp đã tải lên hệ thống (POST /api/v1/files/upload).")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
    }
}

public interface IStepOptionalFields
{
    int? TimerMinutes { get; }

    string? ImageUrl { get; }
}

/// <summary>Bước thêm mới: tiêu đề + mô tả bắt buộc cùng các trường tùy chọn.</summary>
public interface IStepFields : IStepOptionalFields
{
    string? Title { get; }

    string? Description { get; }
}

using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// FR-RCP-004 – cập nhật thông tin cơ bản của công thức với Optimistic Concurrency. Body
/// <c>{ title?, description?, categoryId?, prepTime?, cookTime?, servings?, difficulty?, instructions?, nutrition?, rowVersion }</c>;
/// trường không gửi thì giữ nguyên (instructions = "" để xóa). <see cref="RowVersion"/> lấy từ header <c>If-Match</c> hoặc body.
/// </summary>
public sealed record UpdateRecipeCommand(
    Guid RecipeId,
    string? Title,
    string? Description,
    Guid? CategoryId,
    int? PrepTime,
    int? CookTime,
    int? Servings,
    RecipeDifficulty? Difficulty,
    string? Instructions,
    NutritionInput? Nutrition,
    string? RowVersion) : RecipeWriteCommand, IRequest<RecipeDetailDto>;

public sealed class UpdateRecipeCommandValidator : AbstractValidator<UpdateRecipeCommand>
{
    public UpdateRecipeCommandValidator(IRepository<Category> categories)
    {
        // Không có RowVersion thì không có gì để so — cập nhật "mù" sẽ âm thầm đè thay đổi của người khác.
        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("rowVersion là bắt buộc (header If-Match hoặc trường rowVersion của body).")
            .Must(BeBase64).WithMessage("rowVersion không hợp lệ.")
            .When(x => x.RowVersion is not null, ApplyConditionTo.CurrentValidator);

        // Cùng bộ giới hạn với FR-RCP-003 (SRS §7.9), chỉ áp khi trường được gửi.
        RuleFor(x => x.Title!)
            .Length(RecipeLimits.TitleMin, RecipeLimits.TitleMax)
                .WithMessage($"Tiêu đề phải từ {RecipeLimits.TitleMin} đến {RecipeLimits.TitleMax} ký tự.")
            .Must(RecipeValidationRules.ProducesUsableSlug)
                .WithMessage("Tiêu đề phải chứa ít nhất một chữ cái hoặc chữ số để sinh được đường dẫn.")
            .When(x => x.Title is not null);
        RuleFor(x => x.Description!)
            .Length(RecipeLimits.DescriptionMin, RecipeLimits.DescriptionMax)
                .WithMessage($"Mô tả phải từ {RecipeLimits.DescriptionMin} đến {RecipeLimits.DescriptionMax} ký tự.")
            .When(x => x.Description is not null);
        RuleFor(x => x.Instructions)
            .MaximumLength(RecipeLimits.InstructionsMax).WithMessage($"Hướng dẫn tối đa {RecipeLimits.InstructionsMax} ký tự.");
        RuleFor(x => x.PrepTime).GreaterThan(0).When(x => x.PrepTime.HasValue).WithMessage("Thời gian chuẩn bị phải lớn hơn 0.");
        RuleFor(x => x.CookTime).GreaterThanOrEqualTo(0).When(x => x.CookTime.HasValue).WithMessage("Thời gian nấu phải >= 0.");
        RuleFor(x => x.Servings).GreaterThan(0).When(x => x.Servings.HasValue).WithMessage("Số khẩu phần phải lớn hơn 0.");
        RuleFor(x => x.Difficulty).IsInEnum().When(x => x.Difficulty.HasValue).WithMessage("Độ khó chỉ nhận Easy, Medium, Hard hoặc Expert.");
        RuleFor(x => x.CategoryId!.Value)
            .MustAsync((id, ct) => categories.ExistsAsync(id, ct)).WithMessage("Danh mục không tồn tại.")
            .OverridePropertyName("categoryId")
            .When(x => x.CategoryId.HasValue);
        RuleFor(x => x.Nutrition!).SetValidator(new NutritionInputValidator()).When(x => x.Nutrition is not null);
    }

    private static bool BeBase64(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var buffer = new byte[value.Length];
        return Convert.TryFromBase64String(value, buffer, out var written) && written > 0;
    }
}

public sealed class UpdateRecipeCommandHandler(
    IUnitOfWork unitOfWork,
    IRecipeReadRepository reader,
    IAuthorizationService authorization,
    ICurrentUser currentUser) : IRequestHandler<UpdateRecipeCommand, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(UpdateRecipeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var recipe = await authorization
            .LoadWithDetailsForWriteAsync(currentUser, unitOfWork, request.RecipeId, cancellationToken)
            .ConfigureAwait(false);

        // So RowVersion client đang cầm với DB ngay trong câu UPDATE: lệch → DbUpdateConcurrencyException được
        // RecipePersistenceExceptionTranslator dịch thành 409 RECIPE_CONCURRENCY_CONFLICT. Không try/catch ở đây.
        unitOfWork.Recipes.SetOriginalRowVersion(recipe, Convert.FromBase64String(request.RowVersion!));

        var oldSlug = recipe.Slug;
        var title = request.Title?.Trim() ?? recipe.Title;

        // Slug chỉ sinh lại khi tiêu đề ĐỔI và công thức là bản nháp CHƯA TỪNG xuất bản (MT-27): sau lần publish đầu,
        // link đã chia sẻ/đã lập chỉ mục phải sống mãi — đổi tiêu đề không lỗi, chỉ giữ nguyên slug.
        string? newSlug = null;
        if (!string.Equals(title, recipe.Title, StringComparison.Ordinal) && !recipe.IsSlugLocked)
        {
            newSlug = await RecipeSlugs.ResolveAsync(unitOfWork.Recipes, title, cancellationToken, currentSlug: recipe.Slug).ConfigureAwait(false);
        }

        recipe.Update(
            title,
            request.Description ?? recipe.Description,
            request.CategoryId ?? recipe.CategoryId,
            request.PrepTime ?? recipe.PrepTimeMinutes,
            request.CookTime ?? recipe.CookTimeMinutes,
            request.Servings ?? recipe.Servings,
            request.Difficulty ?? recipe.Difficulty,
            request.Instructions ?? recipe.Instructions);

        if (newSlug is not null && newSlug != oldSlug)
        {
            recipe.ChangeSlug(newSlug);
        }

        if (request.Nutrition is not null)
        {
            recipe.SetNutrition(request.Nutrition.ToEntity());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Xóa cache theo CẢ slug cũ lẫn slug mới (FR-RCP-004 bước 10).
        request.InvalidateOnSuccess(RecipeCacheKeys.ForContentChange(oldSlug));
        if (recipe.Slug != oldSlug)
        {
            request.InvalidateOnSuccess([RecipeCacheKeys.Detail(recipe.Slug)]);
        }

        return await reader.GetDetailByIdAsync(recipe.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Không đọc lại được công thức '{recipe.Id}' sau khi cập nhật.");
    }
}

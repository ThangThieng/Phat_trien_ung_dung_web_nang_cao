using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record NutritionInput(decimal? Calories, decimal? Protein, decimal? Carbohydrates, decimal? Fat, decimal? Fiber, decimal? Sodium)
{
    public RecipeNutrition ToEntity() => new()
    {
        Calories = Calories,
        Protein = Protein,
        Carbohydrates = Carbohydrates,
        Fat = Fat,
        Fiber = Fiber,
        Sodium = Sodium,
    };
}

/// <summary>
/// FR-RCP-003 – tạo công thức ở trạng thái Draft (<c>PublishedAt = NULL</c>). Nutrition đi kèm body vì là Owned Entity
/// (không có endpoint riêng — MT-02).
/// </summary>
public sealed record CreateRecipeCommand(
    string Title,
    string Description,
    Guid CategoryId,
    int PrepTime,
    int CookTime,
    int Servings,
    RecipeDifficulty Difficulty,
    string? Instructions,
    NutritionInput? Nutrition) : IRequest<RecipeDetailDto>, ICacheInvalidator
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate => [RecipeCacheKeys.ListPrefix];
}

/// <summary>Giới hạn độ dài của Recipe theo SRS §7.9 (validator và cột DB bằng nhau — MT-37).</summary>
public static class RecipeLimits
{
    public const int TitleMin = 5;
    public const int TitleMax = 200;
    public const int DescriptionMin = 20;
    public const int DescriptionMax = 2000;
    public const int InstructionsMax = 5000;

    /// <summary>Cột Nutrition_* là numeric(8,2).</summary>
    public const decimal NutritionMax = 999_999.99m;
}

public sealed class CreateRecipeCommandValidator : AbstractValidator<CreateRecipeCommand>
{
    public CreateRecipeCommandValidator(IRepository<Category> categories)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .Length(RecipeLimits.TitleMin, RecipeLimits.TitleMax)
                .WithMessage($"Tiêu đề phải từ {RecipeLimits.TitleMin} đến {RecipeLimits.TitleMax} ký tự.")
            .Must(RecipeValidationRules.ProducesUsableSlug)
                .WithMessage("Tiêu đề phải chứa ít nhất một chữ cái hoặc chữ số để sinh được đường dẫn.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Mô tả không được để trống.")
            .Length(RecipeLimits.DescriptionMin, RecipeLimits.DescriptionMax)
                .WithMessage($"Mô tả phải từ {RecipeLimits.DescriptionMin} đến {RecipeLimits.DescriptionMax} ký tự.");

        RuleFor(x => x.Instructions)
            .MaximumLength(RecipeLimits.InstructionsMax)
                .WithMessage($"Hướng dẫn tối đa {RecipeLimits.InstructionsMax} ký tự.");

        RuleFor(x => x.PrepTime).GreaterThan(0).WithMessage("Thời gian chuẩn bị phải lớn hơn 0.");
        RuleFor(x => x.CookTime).GreaterThanOrEqualTo(0).WithMessage("Thời gian nấu phải >= 0.");
        RuleFor(x => x.Servings).GreaterThan(0).WithMessage("Số khẩu phần phải lớn hơn 0.");
        RuleFor(x => x.Difficulty).IsInEnum().WithMessage("Độ khó chỉ nhận Easy, Medium, Hard hoặc Expert.");

        // FR-RCP-003 A2 (CR-2026-04 c): categoryId không tồn tại là lỗi của TRƯỜNG trong body → 400 gắn vào "categoryId",
        // không phải 404 (FE sẽ hiểu nhầm là URL /recipes không tồn tại). IRepository<Category> generic: không phụ thuộc
        // repository chuyên biệt của module Category.
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Danh mục không được để trống.")
            .MustAsync((id, ct) => categories.ExistsAsync(id, ct)).WithMessage("Danh mục không tồn tại.");

        RuleFor(x => x.Nutrition!)
            .SetValidator(new NutritionInputValidator())
            .When(x => x.Nutrition is not null);
    }
}

public sealed class NutritionInputValidator : AbstractValidator<NutritionInput>
{
    public NutritionInputValidator()
    {
        RuleFor(x => x.Calories).InclusiveBetween(0, RecipeLimits.NutritionMax).When(x => x.Calories.HasValue);
        RuleFor(x => x.Protein).InclusiveBetween(0, RecipeLimits.NutritionMax).When(x => x.Protein.HasValue);
        RuleFor(x => x.Carbohydrates).InclusiveBetween(0, RecipeLimits.NutritionMax).When(x => x.Carbohydrates.HasValue);
        RuleFor(x => x.Fat).InclusiveBetween(0, RecipeLimits.NutritionMax).When(x => x.Fat.HasValue);
        RuleFor(x => x.Fiber).InclusiveBetween(0, RecipeLimits.NutritionMax).When(x => x.Fiber.HasValue);
        RuleFor(x => x.Sodium).InclusiveBetween(0, RecipeLimits.NutritionMax).When(x => x.Sodium.HasValue);
    }
}

public static class RecipeValidationRules
{
    /// <summary>Tiêu đề chỉ gồm ký tự đặc biệt sinh slug rỗng → chặn ở validator để trả 400 thay vì 500.</summary>
    public static bool ProducesUsableSlug(string? value) =>
        string.IsNullOrWhiteSpace(value) || SlugHelper.Generate(value).Length > 0;
}

public sealed class CreateRecipeCommandHandler(
    IUnitOfWork unitOfWork,
    IRecipeReadRepository readRepository,
    ICurrentUser currentUser) : IRequestHandler<CreateRecipeCommand, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Endpoint đã RequireAuthorization(AuthorPolicy) nên luôn có UserId.
        var authorId = currentUser.UserId ?? throw new InvalidOperationException("Endpoint tạo công thức phải yêu cầu xác thực.");
        var slug = await RecipeSlugs.ResolveAsync(unitOfWork.Recipes, request.Title, cancellationToken).ConfigureAwait(false);

        var recipe = Recipe.Create(
            request.Title,
            slug,
            request.Description.Trim(),
            request.CategoryId,
            authorId,
            request.PrepTime,
            request.CookTime,
            request.Servings,
            request.Difficulty,
            request.Instructions);

        if (request.Nutrition is not null)
        {
            recipe.SetNutrition(request.Nutrition.ToEntity());
        }

        await unitOfWork.Recipes.AddAsync(recipe, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await readRepository.GetDetailByIdAsync(recipe.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Không đọc lại được công thức vừa tạo '{recipe.Id}'.");
    }
}

/// <summary>
/// Sinh slug công thức (FR-RCP-003, NFR-SEO-004): từ tiêu đề, không trùng slug dành riêng (search, mine, sitemap, new,
/// edit — MT-53), trùng thì hậu tố -2, -3… Hai request tranh cùng slug vẫn có IDX_Recipe_Slug + RecipeSlugConflictException chặn.
/// </summary>
public static class RecipeSlugs
{
    public static async Task<string> ResolveAsync(IRecipeRepository recipes, string title, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(recipes);

        var baseSlug = Slug.FromText(title);
        var number = 1;
        var candidate = baseSlug;
        while (candidate.IsReserved
            || await recipes.SlugExistsAsync(candidate.Value, cancellationToken).ConfigureAwait(false))
        {
            number++;
            candidate = baseSlug.WithSuffix(number);
        }

        return candidate.Value;
    }
}

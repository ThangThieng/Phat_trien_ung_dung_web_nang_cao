using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Files;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record NutritionInput(decimal? Calories, decimal? Protein, decimal? Carbohydrates, decimal? Fat, decimal? Fiber, decimal? Sodium);
public sealed record CreateRecipeCommand(string Title, string Description, Guid CategoryId, int PrepTime, int CookTime, int Servings, RecipeDifficulty Difficulty, string? Instructions, NutritionInput? Nutrition) : IRequest<RecipeDetailDto>, ICacheInvalidator
{ public IReadOnlyCollection<string> CacheKeysToInvalidate => ["recipes:list:"]; }

public sealed class CreateRecipeCommandValidator : AbstractValidator<CreateRecipeCommand>
{
    public CreateRecipeCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(5, 200);
        RuleFor(x => x.Description).NotEmpty().Length(20, 2000);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.PrepTime).GreaterThan(0);
        RuleFor(x => x.CookTime).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Servings).GreaterThan(0);
        RuleFor(x => x.Instructions).MaximumLength(10000);
    }
}

public sealed class CreateRecipeCommandHandler(IRecipeWriteRepository recipes, IUnitOfWork unitOfWork, ICurrentUser currentUser) : IRequestHandler<CreateRecipeCommand, RecipeDetailDto>
{
    private static readonly HashSet<string> ReservedSlugs = ["search", "mine", "sitemap", "new", "edit"];
    public async Task<RecipeDetailDto> Handle(CreateRecipeCommand request, CancellationToken ct)
    {
        var authorId = currentUser.UserId ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Yêu cầu đăng nhập.");
        if (!await recipes.CategoryExistsAsync(request.CategoryId, ct).ConfigureAwait(false)) throw new NotFoundException(ErrorCodes.CategoryNotFound, "Danh mục không tồn tại.");
        var root = SlugHelper.Generate(request.Title);
        if (ReservedSlugs.Contains(root)) throw new BadRequestException(ErrorCodes.ValidationError, "Tiêu đề tạo slug thuộc từ khóa dành riêng.");
        var slug = root; var suffix = 2;
        while (await recipes.SlugExistsAsync(slug, ct).ConfigureAwait(false)) slug = $"{root}-{suffix++}";
        var recipe = Recipe.Create(request.Title, slug, request.Description, request.CategoryId, authorId, request.PrepTime, request.CookTime, request.Servings, request.Difficulty, request.Instructions);
        if (request.Nutrition is { } n) recipe.SetNutrition(new RecipeNutrition { Calories=n.Calories, Protein=n.Protein, Carbohydrates=n.Carbohydrates, Fat=n.Fat, Fiber=n.Fiber, Sodium=n.Sodium });
        await recipes.AddAsync(recipe, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        return new RecipeDetailDto(recipe.Id, recipe.Title, recipe.Slug, recipe.Description, recipe.Instructions, recipe.PrepTimeMinutes, recipe.CookTimeMinutes, recipe.Servings, recipe.Difficulty, recipe.Status, new CategoryRefDto(recipe.CategoryId, string.Empty, string.Empty), new AuthorDto(authorId, string.Empty, null), null, [], [], [], null, recipe.CreatedAt, recipe.UpdatedAt, Convert.ToBase64String(recipe.RowVersion));
    }
}

public sealed record UploadRecipeImageCommand(Guid RecipeId, Stream Content, long Length, string? ContentType, string? AltText) : IRequest<RecipeImageDto>, ICacheInvalidator
{ public IReadOnlyCollection<string> CacheKeysToInvalidate => ["recipes:list:"]; }

public sealed class UploadRecipeImageCommandHandler(IRecipeWriteRepository recipes, IFileStorageService storage, IUnitOfWork unitOfWork, ICurrentUser currentUser) : IRequestHandler<UploadRecipeImageCommand, RecipeImageDto>
{
    public async Task<RecipeImageDto> Handle(UploadRecipeImageCommand request, CancellationToken ct)
    {
        var recipe = await RequireOwnedRecipe(recipes, currentUser, request.RecipeId, ct).ConfigureAwait(false);
        if (request.Length <= 0 || request.Length > ImageFileInspector.MaxFileSizeBytes) throw new BadRequestException(ErrorCodes.FileSizeExceeded, "Kích thước file vượt quá giới hạn 5MB.");
        if (!ImageFileInspector.IsAllowedContentType(request.ContentType)) throw new BadRequestException(ErrorCodes.FileMimeInvalid, "Chỉ chấp nhận JPEG, PNG, WebP, AVIF.");
        var stored = await storage.UploadAsync(request.Content, $"recipes/{recipe.Id}", Path.GetExtension("." + request.ContentType!.Split('/').Last()).Replace(".jpeg", ".jpg"), request.ContentType!, ct).ConfigureAwait(false);
        var image = recipe.AddImage(stored.Url, request.AltText);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        return ToDto(image);
    }
    internal static async Task<Recipe> RequireOwnedRecipe(IRecipeWriteRepository recipes, ICurrentUser user, Guid id, CancellationToken ct)
    {
        var recipe = await recipes.GetForWriteAsync(id, ct).ConfigureAwait(false) ?? throw new NotFoundException(ErrorCodes.RecipeNotFound, "Công thức không tồn tại.");
        if (!user.IsAdmin && user.UserId != recipe.AuthorId) throw new ForbiddenException(ErrorCodes.RecipeForbidden, "Bạn không có quyền sửa công thức này.");
        return recipe;
    }
    internal static RecipeImageDto ToDto(RecipeImage image) => new(image.Id, image.OriginalUrl, image.MediumUrl, image.ThumbnailUrl, image.AltText, image.IsPrimary, image.OrderIndex);
}

public sealed record UpdateRecipeImageCommand(Guid RecipeId, Guid ImageId, string? AltText, int? OrderIndex, bool? IsPrimary) : IRequest<RecipeImageDto>, ICacheInvalidator { public IReadOnlyCollection<string> CacheKeysToInvalidate => ["recipes:list:"]; }
public sealed class UpdateRecipeImageCommandHandler(IRecipeWriteRepository recipes, IUnitOfWork unitOfWork, ICurrentUser currentUser) : IRequestHandler<UpdateRecipeImageCommand, RecipeImageDto>
{
    public async Task<RecipeImageDto> Handle(UpdateRecipeImageCommand r, CancellationToken ct) { var recipe=await UploadRecipeImageCommandHandler.RequireOwnedRecipe(recipes,currentUser,r.RecipeId,ct).ConfigureAwait(false); var image=recipe.Images.SingleOrDefault(x=>x.Id==r.ImageId) ?? throw new NotFoundException(ErrorCodes.RecipeImageNotFound,"Ảnh không tồn tại."); recipe.UpdateImage(image,r.AltText,r.OrderIndex,r.IsPrimary); await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false); return UploadRecipeImageCommandHandler.ToDto(image); }
}

public sealed record DeleteRecipeImageCommand(Guid RecipeId, Guid ImageId) : IRequest, ICacheInvalidator { public IReadOnlyCollection<string> CacheKeysToInvalidate => ["recipes:list:"]; }
public sealed class DeleteRecipeImageCommandHandler(IRecipeWriteRepository recipes, IFileStorageService storage, IUnitOfWork unitOfWork, ICurrentUser currentUser) : IRequestHandler<DeleteRecipeImageCommand>
{
    public async Task Handle(DeleteRecipeImageCommand r, CancellationToken ct) { var recipe=await UploadRecipeImageCommandHandler.RequireOwnedRecipe(recipes,currentUser,r.RecipeId,ct).ConfigureAwait(false); var image=recipe.Images.SingleOrDefault(x=>x.Id==r.ImageId) ?? throw new NotFoundException(ErrorCodes.RecipeImageNotFound,"Ảnh không tồn tại."); var url=image.OriginalUrl; recipe.RemoveImage(image); recipes.RemoveImage(image); await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false); await storage.DeleteAsync(url,ct).ConfigureAwait(false); }
}

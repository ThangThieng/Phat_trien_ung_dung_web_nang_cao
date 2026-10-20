using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace CulinaryBlog.Application.Features.Recipes;

// ================= FR-RCP-009 — nguyên liệu =================

/// <summary>FR-RCP-009 – thêm nguyên liệu (201 RecipeIngredientDto). Trường phẳng như body để lỗi 400 nằm đúng khóa của ô.</summary>
public sealed record AddIngredientCommand(
    Guid RecipeId,
    string? Name,
    decimal? Quantity,
    string? QuantityText,
    string? Unit,
    string? Notes,
    int? OrderIndex) : RecipeWriteCommand, IRequest<RecipeIngredientDto>, IIngredientInputFields;

public sealed class AddIngredientCommandValidator : IngredientFieldsValidator<AddIngredientCommand>;

/// <summary>
/// FR-RCP-009 – cập nhật nguyên liệu (PUT <c>{ name?, quantity?, quantityText?, unit?, notes?, orderIndex? }</c>).
/// <c>name</c>/<c>orderIndex</c> không gửi thì giữ nguyên; bộ định lượng + ghi chú được THAY như một khối (null = xóa) —
/// đó là cách duy nhất để chuyển "500 gram" thành "vừa đủ" (bỏ quantity) mà không cần cú pháp "xóa trường" riêng.
/// </summary>
public sealed record UpdateIngredientCommand(
    Guid RecipeId,
    Guid IngredientId,
    string? Name,
    decimal? Quantity,
    string? QuantityText,
    string? Unit,
    string? Notes,
    int? OrderIndex) : RecipeWriteCommand, IRequest<RecipeIngredientDto>, IIngredientFields;

public sealed class UpdateIngredientCommandValidator : AbstractValidator<UpdateIngredientCommand>
{
    public UpdateIngredientCommandValidator()
    {
        RuleFor(x => x.Name!)
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Tên nguyên liệu không được để trống.")
            .MaximumLength(RecipeContentLimits.IngredientNameMax)
                .WithMessage($"Tên nguyên liệu tối đa {RecipeContentLimits.IngredientNameMax} ký tự.")
            .When(x => x.Name is not null);
        IngredientRules.Apply(this);
    }
}

public sealed record DeleteIngredientCommand(Guid RecipeId, Guid IngredientId) : RecipeWriteCommand, IRequest;

public sealed class AddIngredientCommandHandler(RecipeContent content) : IRequestHandler<AddIngredientCommand, RecipeIngredientDto>
{
    public async Task<RecipeIngredientDto> Handle(AddIngredientCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var added = await content
            .ApplyAsync(
                request,
                request.RecipeId,
                r => r.AddIngredient(
                    new IngredientDetails(request.Name ?? string.Empty, request.Quantity, request.QuantityText, request.Unit, request.Notes),
                    request.OrderIndex),
                cancellationToken)
            .ConfigureAwait(false);
        return RecipeIngredientDto.From(added);
    }
}

public sealed class UpdateIngredientCommandHandler(RecipeContent content) : IRequestHandler<UpdateIngredientCommand, RecipeIngredientDto>
{
    public async Task<RecipeIngredientDto> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var updated = await content
            .ApplyAsync(
                request,
                request.RecipeId,
                r =>
                {
                    var current = r.GetIngredient(request.IngredientId);
                    var details = new IngredientDetails(request.Name ?? current.Name, request.Quantity, request.QuantityText, request.Unit, request.Notes);
                    return r.UpdateIngredient(request.IngredientId, details, request.OrderIndex);
                },
                cancellationToken)
            .ConfigureAwait(false);
        return RecipeIngredientDto.From(updated);
    }
}

public sealed class DeleteIngredientCommandHandler(RecipeContent content) : IRequestHandler<DeleteIngredientCommand>
{
    public Task Handle(DeleteIngredientCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return content.ApplyAsync(
            request,
            request.RecipeId,
            r =>
            {
                r.RemoveIngredient(request.IngredientId);
                return true;
            },
            cancellationToken);
    }
}

// ================= FR-RCP-010 — bước nấu =================

/// <summary>FR-RCP-010 – thêm bước; server gán StepNumber = Max + 1 (201 RecipeStepDto).</summary>
public sealed record AddStepCommand(Guid RecipeId, string? Title, string? Description, int? TimerMinutes, string? ImageUrl)
    : RecipeWriteCommand, IRequest<RecipeStepDto>, IStepFields;

public sealed class AddStepCommandValidator(IFileStorageService storage) : StepFieldsValidator<AddStepCommand>(storage);

/// <summary>
/// FR-RCP-010 – cập nhật NỘI DUNG một bước (PUT <c>{ title?, description?, timerMinutes?, imageUrl? }</c>, không có
/// stepNumber). title/description không gửi thì giữ nguyên; timerMinutes/imageUrl được thay (null = bỏ).
/// </summary>
public sealed record UpdateStepCommand(Guid RecipeId, Guid StepId, string? Title, string? Description, int? TimerMinutes, string? ImageUrl)
    : RecipeWriteCommand, IRequest<RecipeStepDto>, IStepOptionalFields;

public sealed class UpdateStepCommandValidator : AbstractValidator<UpdateStepCommand>
{
    public UpdateStepCommandValidator(IFileStorageService storage)
    {
        RuleFor(x => x.Title!)
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Tiêu đề bước không được để trống.")
            .MaximumLength(RecipeContentLimits.StepTitleMax).WithMessage($"Tiêu đề bước tối đa {RecipeContentLimits.StepTitleMax} ký tự.")
            .When(x => x.Title is not null);
        RuleFor(x => x.Description!)
            .Must(d => !string.IsNullOrWhiteSpace(d)).WithMessage("Mô tả bước không được để trống.")
            .MaximumLength(RecipeContentLimits.StepDescriptionMax)
                .WithMessage($"Mô tả bước tối đa {RecipeContentLimits.StepDescriptionMax} ký tự.")
            .When(x => x.Description is not null);
        StepRules.Apply(this, storage);
    }
}

public sealed record DeleteStepCommand(Guid RecipeId, Guid StepId) : RecipeWriteCommand, IRequest;

/// <summary>FR-RCP-010 bước 9–11 – sắp xếp lại: mảng ĐẦY ĐỦ id theo thứ tự mới → StepNumber 1..N trong một transaction.</summary>
public sealed record ReorderStepsCommand(Guid RecipeId, IReadOnlyList<Guid> StepIds) : RecipeWriteCommand, IRequest<IReadOnlyList<RecipeStepDto>>;

public sealed class ReorderStepsCommandValidator : AbstractValidator<ReorderStepsCommand>
{
    public ReorderStepsCommandValidator()
    {
        RuleFor(x => x.StepIds)
            .NotEmpty().WithMessage("stepIds không được rỗng.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("stepIds không được chứa id trùng.");
    }
}

public sealed class AddStepCommandHandler(RecipeContent content) : IRequestHandler<AddStepCommand, RecipeStepDto>
{
    public async Task<RecipeStepDto> Handle(AddStepCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var added = await content
            .ApplyAsync(request, request.RecipeId, r => r.AddStep(request.Title!, request.Description!, request.TimerMinutes, request.ImageUrl), cancellationToken)
            .ConfigureAwait(false);
        return RecipeStepDto.From(added);
    }
}

public sealed class UpdateStepCommandHandler(RecipeContent content) : IRequestHandler<UpdateStepCommand, RecipeStepDto>
{
    public async Task<RecipeStepDto> Handle(UpdateStepCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var updated = await content
            .ApplyAsync(
                request,
                request.RecipeId,
                r =>
                {
                    var current = r.GetStep(request.StepId);
                    return r.UpdateStep(
                        request.StepId,
                        request.Title ?? current.Title,
                        request.Description ?? current.Description,
                        request.TimerMinutes,
                        request.ImageUrl);
                },
                cancellationToken)
            .ConfigureAwait(false);
        return RecipeStepDto.From(updated);
    }
}

public sealed class DeleteStepCommandHandler(RecipeContent content) : IRequestHandler<DeleteStepCommand>
{
    public Task Handle(DeleteStepCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return content.ApplyAsync(
            request,
            request.RecipeId,
            r =>
            {
                r.RemoveStep(request.StepId);
                return true;
            },
            cancellationToken);
    }
}

public sealed class ReorderStepsCommandHandler(RecipeContent content) : IRequestHandler<ReorderStepsCommand, IReadOnlyList<RecipeStepDto>>
{
    public async Task<IReadOnlyList<RecipeStepDto>> Handle(ReorderStepsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var ordered = await content
            .ApplyAsync(request, request.RecipeId, r => r.ReorderSteps(request.StepIds), cancellationToken)
            .ConfigureAwait(false);
        return [.. ordered.Select(RecipeStepDto.From)];
    }
}

/// <summary>
/// Luồng chung của mọi command NỘI DUNG (Buổi 4 — Dev 2): nạp aggregate kèm bước + nguyên liệu + ảnh → 404 → 403
/// RECIPE_FORBIDDEN (RecipeAuthorizationHandler) → thao tác Domain → MỘT SaveChanges (đánh số lại bước dựa vào
/// constraint DEFERRABLE — D-16) → khai báo khóa cache <c>recipe:{slug}</c> + tiền tố danh sách/tìm kiếm.
/// </summary>
public sealed class RecipeContent(IUnitOfWork unitOfWork, IAuthorizationService authorization, ICurrentUser currentUser)
{
    public async Task<TResult> ApplyAsync<TResult>(
        RecipeWriteCommand command,
        Guid recipeId,
        Func<Recipe, TResult> change,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(change);

        var recipe = await authorization
            .LoadWithDetailsForWriteAsync(currentUser, unitOfWork, recipeId, cancellationToken)
            .ConfigureAwait(false);

        var result = change(recipe);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        command.InvalidateOnSuccess(RecipeCacheKeys.ForContentChange(recipe.Slug));
        return result;
    }
}

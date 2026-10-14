using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Exceptions.Auth;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>SRS §8.1 – <c>UserAdminDto { id, email, displayName, avatarUrl, roles, isActive, createdAt, recipeCount }</c>; không có PasswordHash/SecurityStamp/UserName.</summary>
public sealed record UserAdminDto(
    string Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    IReadOnlyList<string> Roles,
    bool IsActive,
    DateTime CreatedAt,
    int RecipeCount);

public sealed record UserListCriteria(int Page, int PageSize, string? Search, bool? IsActive);

/// <summary>SRS §8.1 – response khóa/mở khóa: <c>{ id, email, displayName, isActive }</c>.</summary>
public sealed record UserStatusDto(string Id, string Email, string DisplayName, bool IsActive);

/// <summary>
/// FR-AUTH-008 – Admin tìm người dùng để khóa/mở khóa (<c>GET /users?page&amp;pageSize&amp;search&amp;isActive</c>).
/// KHÔNG cài ICacheable: dữ liệu quản trị, đổi ngay sau mỗi thao tác khóa (endpoint trả Cache-Control: no-store).
/// </summary>
public sealed record GetUsersQuery(int Page = 1, int PageSize = 12, string? Search = null, bool? IsActive = null)
    : IRequest<PagedResult<UserAdminDto>>;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("page phải >= 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage("pageSize phải trong khoảng 1–50.");
        RuleFor(x => x.Search).MaximumLength(100).WithMessage("search tối đa 100 ký tự.");
    }
}

public sealed class GetUsersQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetUsersQuery, PagedResult<UserAdminDto>>
{
    public Task<PagedResult<UserAdminDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
        return unitOfWork.Users.GetUsersPageAsync(
            new UserListCriteria(request.Page, request.PageSize, search, request.IsActive),
            cancellationToken);
    }
}

/// <summary>FR-AUTH-008 – khóa (<c>isActive = false</c>) / mở khóa một tài khoản, kèm lý do để ghi audit log.</summary>
/// <remarks><see cref="IsActive"/> nullable có chủ đích: body thiếu trường này phải là 400, không được ngầm hiểu thành <c>false</c> (khóa nhầm).</remarks>
public sealed record SetUserStatusCommand(string UserId, bool? IsActive, string? Reason) : IRequest<UserStatusDto>;

public sealed class SetUserStatusCommandValidator : AbstractValidator<SetUserStatusCommand>
{
    public SetUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(450);
        RuleFor(x => x.IsActive).NotNull().WithMessage("isActive là bắt buộc (true = mở khóa, false = khóa).");
        RuleFor(x => x.Reason).MaximumLength(500).WithMessage("reason tối đa 500 ký tự.");
    }
}

/// <summary>
/// Khóa = gán IsActive (qua UserManager) + thu hồi MỌI refresh token còn hiệu lực ("force revoke") trong CÙNG một
/// transaction: không thể có trạng thái "đã khóa nhưng phiên vẫn refresh được". Access token đã cấp còn sống tối đa
/// 15 phút — đánh đổi có chủ đích của JWT stateless (CONS-004).
/// </summary>
public sealed partial class SetUserStatusCommandHandler(
    IIdentityService identityService,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<SetUserStatusCommandHandler> logger) : IRequestHandler<SetUserStatusCommand, UserStatusDto>
{
    public async Task<UserStatusDto> Handle(SetUserStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var adminId = currentUser.UserId ?? throw new InvalidTokenException();

        // A2 (MT-52): 403 — request hợp lệ, không xung đột trạng thái; chỉ là không được làm việc này lên chính mình.
        // Admin tự khóa mình sẽ mất quyền quản trị ngay và có thể không còn Admin nào mở khóa lại.
        if (string.Equals(adminId, request.UserId, StringComparison.Ordinal))
        {
            throw new ForbiddenException(HttpProblemTypes.Forbidden, "Quản trị viên không thể tự khóa hoặc mở khóa tài khoản của chính mình.");
        }

        IdentityUserInfo? user = null;
        var revoked = 0;
        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                user = await identityService.SetActiveAsync(request.UserId, request.IsActive!.Value, ct).ConfigureAwait(false)
                    ?? throw new UserNotFoundException(request.UserId);

                if (request.IsActive == false)
                {
                    revoked = await unitOfWork.Users
                        .RevokeAllRefreshTokensAsync(request.UserId, timeProvider.GetUtcNow().UtcDateTime, ct)
                        .ConfigureAwait(false);
                }
            },
            cancellationToken).ConfigureAwait(false);

        // NFR-SEC-006: audit log ai thao tác, lên ai, lý do gì (timestamp do Serilog gắn).
        if (request.IsActive == true)
        {
            LogReactivated(logger, request.UserId, adminId, request.Reason);
        }
        else
        {
            LogDeactivated(logger, request.UserId, adminId, request.Reason, revoked);
        }

        return new UserStatusDto(user!.Id, user.Email, user.DisplayName, user.IsActive);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "ADMIN ACTION: user {TargetUserId} deactivated by {AdminId}, reason: {Reason}; revoked {RevokedCount} refresh token(s)")]
    private static partial void LogDeactivated(ILogger logger, string targetUserId, string adminId, string? reason, int revokedCount);

    [LoggerMessage(Level = LogLevel.Warning, Message = "ADMIN ACTION: user {TargetUserId} reactivated by {AdminId}, reason: {Reason}")]
    private static partial void LogReactivated(ILogger logger, string targetUserId, string adminId, string? reason);
}

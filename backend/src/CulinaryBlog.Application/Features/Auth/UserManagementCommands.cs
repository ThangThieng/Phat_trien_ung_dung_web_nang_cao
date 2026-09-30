using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth;

public sealed record GetUsersQuery(int Page, int PageSize, string? Search, bool? IsActive) : IRequest<PagedResult<UserAdminDto>>;

public sealed class GetUsersQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUsersQuery, PagedResult<UserAdminDto>>
{
    public Task<PagedResult<UserAdminDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        if (request.Page < 1 || request.PageSize is < 1 or > 50 || request.Search?.Length > 100)
        {
            throw new BadRequestException(ErrorCodes.ValidationError, "page phải từ 1 trở lên, pageSize từ 1 đến 50 và search không quá 100 ký tự.");
        }

        return identityService.GetUsersAsync(request.Page, request.PageSize, request.Search, request.IsActive, cancellationToken);
    }
}

public sealed record SetUserStatusCommand(string UserId, bool IsActive, string? Reason) : IRequest<UserStatusDto>;

public sealed record UserStatusDto(string Id, string Email, string DisplayName, bool IsActive);

public sealed partial class SetUserStatusCommandHandler(ICurrentUser currentUser, IIdentityService identityService, ILogger<SetUserStatusCommandHandler> logger)
    : IRequestHandler<SetUserStatusCommand, UserStatusDto>
{
    public async Task<UserStatusDto> Handle(SetUserStatusCommand request, CancellationToken cancellationToken)
    {
        var actorId = currentUser.UserId ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Yêu cầu đăng nhập.");
        if (!request.IsActive && string.Equals(actorId, request.UserId, StringComparison.Ordinal))
        {
            throw new ForbiddenException("about:blank", "Admin không thể tự khóa tài khoản của mình.");
        }

        if (request.Reason?.Length > 500)
        {
            throw new BadRequestException(ErrorCodes.ValidationError, "Lý do không được vượt quá 500 ký tự.");
        }

        var user = await identityService.SetActiveAsync(request.UserId, request.IsActive, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(ErrorCodes.AuthUserNotFound, "Không tìm thấy người dùng.");
        LogAccountStatusChanged(logger, actorId, user.Id, request.IsActive, request.Reason);
        return new UserStatusDto(user.Id, user.Email, user.DisplayName, user.IsActive);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "AUDIT: account status changed by {ActorUserId} for {TargetUserId}; IsActive={IsActive}; Reason={Reason}")]
    private static partial void LogAccountStatusChanged(ILogger logger, string actorUserId, string targetUserId, bool isActive, string? reason);
}

public sealed record GetMySessionsQuery : IRequest<IReadOnlyList<SessionDto>>;

public sealed class GetMySessionsQueryHandler(ICurrentUser currentUser, IRefreshTokenRepository refreshTokens, TimeProvider timeProvider)
    : IRequestHandler<GetMySessionsQuery, IReadOnlyList<SessionDto>>
{
    public async Task<IReadOnlyList<SessionDto>> Handle(GetMySessionsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Yêu cầu đăng nhập.");
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var currentSessionId = Guid.TryParse(currentUser.SessionId, out var id) ? id : Guid.Empty;
        var tokens = await refreshTokens.GetActiveForUserAsync(userId, now, cancellationToken).ConfigureAwait(false);
        return tokens.Select(token => new SessionDto(token.Id, token.CreatedAt, token.CreatedByIp, token.ExpiresAt, token.Id == currentSessionId)).ToArray();
    }
}

public sealed record RevokeMySessionCommand(Guid SessionId) : IRequest;

public sealed class RevokeMySessionCommandHandler(ICurrentUser currentUser, IRefreshTokenRepository refreshTokens, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : IRequestHandler<RevokeMySessionCommand>
{
    public async Task Handle(RevokeMySessionCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Yêu cầu đăng nhập.");
        var session = await refreshTokens.GetByIdForUserAsync(request.SessionId, userId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("about:blank", "Không tìm thấy phiên đăng nhập.");
        if (session.RevokedAt is null)
        {
            session.Revoke(timeProvider.GetUtcNow().UtcDateTime);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

public sealed record RevokeAllMySessionsCommand : IRequest;

public sealed class RevokeAllMySessionsCommandHandler(ICurrentUser currentUser, IRefreshTokenRepository refreshTokens, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : IRequestHandler<RevokeAllMySessionsCommand>
{
    public async Task Handle(RevokeAllMySessionsCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Yêu cầu đăng nhập.");
        await refreshTokens.RevokeAllForUserAsync(userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

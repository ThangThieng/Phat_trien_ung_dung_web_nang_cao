using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth;

public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress) : IRequest<AuthResponseDto>;

public sealed partial class RefreshTokenCommandHandler(
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokens,
    IIdentityService identityService,
    AuthResponseFactory authResponseFactory,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<RefreshTokenCommandHandler> logger) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Refresh token không hợp lệ.");
        }

        var token = await refreshTokens.GetByHashAsync(tokenService.HashToken(request.RefreshToken), cancellationToken).ConfigureAwait(false);
        if (token is null)
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Refresh token không hợp lệ.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (token.ExpiresAt <= now)
        {
            throw new UnauthorizedException(ErrorCodes.AuthRefreshTokenExpired, "Refresh token đã hết hạn.");
        }

        var user = await identityService.GetByIdAsync(token.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Tài khoản của refresh token không còn tồn tại.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException(ErrorCodes.AuthAccountDisabled, "Tài khoản đã bị vô hiệu hóa.");
        }

        if (await identityService.IsLockedOutAsync(user.Id, cancellationToken).ConfigureAwait(false))
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Tài khoản hiện đang bị khóa.");
        }

        if (token.RevokedAt is not null)
        {
            await refreshTokens.RevokeAllForUserAsync(token.UserId, now, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            LogRefreshTokenReuse(logger, token.UserId, request.IpAddress);
            throw new UnauthorizedException(ErrorCodes.AuthRefreshTokenRevoked, "Refresh token đã bị thu hồi; vui lòng đăng nhập lại.");
        }

        if (!token.IsActive(now))
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Refresh token không hợp lệ.");
        }

        var replacement = tokenService.CreateRefreshToken();
        token.Revoke(now, replacement.TokenHash);
        return await authResponseFactory.IssueAsync(user, request.IpAddress, cancellationToken, replacement).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "SECURITY ALERT: refresh token reuse detected for user {UserId} from IP {ipAddress}")]
    private static partial void LogRefreshTokenReuse(ILogger logger, string userId, string? ipAddress);
}

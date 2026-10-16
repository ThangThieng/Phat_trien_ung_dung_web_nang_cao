using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth;

public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress) : IRequest<AuthResponseDto>;

public sealed partial class RefreshTokenCommandHandler(
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokens,
    IIdentityService identityService,
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
            await RejectReuseAsync(token, now, request.IpAddress, cancellationToken).ConfigureAwait(false);
        }

        if (!token.IsActive(now))
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Refresh token không hợp lệ.");
        }

        var replacement = tokenService.CreateRefreshToken();
        var sessionId = Guid.NewGuid();
        var accessToken = tokenService.CreateAccessToken(user, sessionId);
        var replacementEntity = RefreshToken.Create(user.Id, replacement.TokenHash, replacement.ExpiresAt, now, request.IpAddress, sessionId);
        if (!await refreshTokens.TryRotateAsync(token, replacementEntity, now, cancellationToken).ConfigureAwait(false))
        {
            await RejectReuseAsync(token, now, request.IpAddress, cancellationToken).ConfigureAwait(false);
        }

        return new AuthResponseDto(accessToken.Token, replacement.RawToken, accessToken.ExpiresAt, accessToken.ExpiresInSeconds, user.ToDto());
    }

    private async Task RejectReuseAsync(RefreshToken token, DateTime now, string? ipAddress, CancellationToken cancellationToken)
    {
        await refreshTokens.RevokeFamilyAsync(token.UserId, token.TokenHash, now, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        LogRefreshTokenReuse(logger, token.UserId, ipAddress);
        throw new UnauthorizedException(ErrorCodes.AuthRefreshTokenRevoked, "Refresh token đã bị thu hồi; vui lòng đăng nhập lại.");
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "SECURITY ALERT: refresh token reuse detected for user {UserId} from IP {ipAddress}")]
    private static partial void LogRefreshTokenReuse(ILogger logger, string userId, string? ipAddress);
}

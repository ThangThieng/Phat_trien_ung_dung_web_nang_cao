using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions.Auth;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth;

public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress) : IRequest<AuthResponseDto>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty().OverridePropertyName("refreshToken");
    }
}

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
        var token = await refreshTokens.GetByHashAsync(tokenService.HashToken(request.RefreshToken), cancellationToken).ConfigureAwait(false);
        if (token is null)
        {
            throw new InvalidTokenException("Refresh token không hợp lệ.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (token.ExpiresAt <= now)
        {
            throw InvalidTokenException.RefreshTokenExpired();
        }

        var response = await refreshTokens.ExecuteForUserAsync<AuthResponseDto?>(
            token.UserId,
            async ct =>
            {
                now = timeProvider.GetUtcNow().UtcDateTime;
                if (token.ExpiresAt <= now)
                {
                    throw InvalidTokenException.RefreshTokenExpired();
                }

                var user = await identityService.GetByIdAsync(token.UserId, ct).ConfigureAwait(false);
                if (user is null)
                {
                    throw new InvalidTokenException("Tài khoản của refresh token không còn tồn tại.");
                }

                if (!user.IsActive)
                {
                    throw new AccountDisabledException();
                }

                if (await identityService.IsLockedOutAsync(user.Id, ct).ConfigureAwait(false))
                {
                    throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Tài khoản hiện đang bị khóa.");
                }

                if (token.RevokedAt is not null)
                {
                    await RevokeReusedFamilyAsync(token, now, ct).ConfigureAwait(false);
                    return null;
                }

                if (!token.IsActive(now))
                {
                    throw new InvalidTokenException("Refresh token không hợp lệ.");
                }

                var replacement = tokenService.CreateRefreshToken();
                var sessionId = Guid.NewGuid();
                var accessToken = tokenService.CreateAccessToken(user, sessionId);
                var replacementEntity = RefreshToken.Create(user.Id, replacement.TokenHash, replacement.ExpiresAt, now, request.IpAddress, sessionId);
                if (!await refreshTokens.TryRotateAsync(token, replacementEntity, now, ct).ConfigureAwait(false))
                {
                    await RevokeReusedFamilyAsync(token, now, ct).ConfigureAwait(false);
                    return null;
                }

                return new AuthResponseDto(accessToken.Token, replacement.RawToken, accessToken.ExpiresAt, accessToken.ExpiresInSeconds, user.ToDto());
            },
            cancellationToken).ConfigureAwait(false);
        if (response is null)
        {
            // Commit family revocation before reporting reuse; throwing inside the transaction would undo it.
            LogRefreshTokenReuse(logger, token.UserId, request.IpAddress);
            throw InvalidTokenException.RefreshTokenRevoked();
        }

        return response;
    }

    private async Task RevokeReusedFamilyAsync(RefreshToken token, DateTime now, CancellationToken cancellationToken)
    {
        await refreshTokens.RevokeFamilyAsync(token.UserId, token.TokenHash, now, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "SECURITY ALERT: refresh token reuse detected for user {UserId} from IP {ipAddress}")]
    private static partial void LogRefreshTokenReuse(ILogger logger, string userId, string? ipAddress);
}

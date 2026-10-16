using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Auth;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>
/// FR-AUTH-004 – làm mới access token bằng refresh token, có Token Rotation và Reuse Detection. Không cần Bearer:
/// access token đã hết hạn thì không dùng để xác thực được — chính refresh token là bằng chứng.
/// </summary>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>
{
    public string? IpAddress { get; init; }
}

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().OverridePropertyName("refreshToken");
    }
}

/// <summary>
/// Refresh, issuance and revocation share the same PostgreSQL user-row lock. Reuse revokes only
/// descendants of the reused token; an independent login session remains valid.
/// </summary>
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
        ArgumentNullException.ThrowIfNull(request);

        var token = await refreshTokens.GetByHashAsync(tokenService.HashToken(request.RefreshToken), cancellationToken)
            .ConfigureAwait(false) ?? throw new InvalidTokenException();

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

                var user = await identityService.FindByIdAsync(token.UserId, ct).ConfigureAwait(false)
                    ?? throw new InvalidTokenException();
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
                    throw new InvalidTokenException();
                }

                var replacement = tokenService.CreateRefreshToken();
                var session = RefreshToken.Create(user.Id, replacement.TokenHash, replacement.ExpiresAt, now, request.IpAddress);
                if (!await refreshTokens.TryRotateAsync(token, session, now, ct).ConfigureAwait(false))
                {
                    await RevokeReusedFamilyAsync(token, now, ct).ConfigureAwait(false);
                    return null;
                }

                var accessToken = tokenService.CreateAccessToken(user, session.Id);
                return new AuthResponseDto(accessToken.Token, replacement.RawToken, accessToken.ExpiresAt, accessToken.ExpiresInSeconds, user.ToDto());
            },
            cancellationToken).ConfigureAwait(false);

        if (response is null)
        {
            LogReuseDetected(logger, token.UserId, request.IpAddress);
            throw InvalidTokenException.RefreshTokenRevoked();
        }

        return response;
    }

    private async Task RevokeReusedFamilyAsync(RefreshToken token, DateTime now, CancellationToken cancellationToken)
    {
        await refreshTokens.RevokeFamilyAsync(token.UserId, token.TokenHash, now, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "SECURITY ALERT: refresh token reuse detected for user {UserId} from IP {Ip}; affected token family revoked")]
    private static partial void LogReuseDetected(ILogger logger, string userId, string? ip);
}

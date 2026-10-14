using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions.Auth;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>Phát cặp token cho user và lưu refresh token (dùng chung cho Register/Login, sau này Google/Refresh).</summary>
public sealed class AuthResponseFactory(
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokens,
    IIdentityService identityService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<AuthResponseDto> IssueAsync(IdentityUserInfo user, string? ipAddress, CancellationToken cancellationToken, GeneratedRefreshToken? generatedRefreshToken = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        return await refreshTokens.ExecuteForUserAsync(
            user.Id,
            async ct =>
            {
                // Login/Google may have read the user before a concurrent disable committed.
                var current = await identityService.GetByIdAsync(user.Id, ct).ConfigureAwait(false)
                    ?? throw new InvalidTokenException("Tài khoản không còn tồn tại.");
                if (!current.IsActive)
                {
                    throw new AccountDisabledException();
                }

                var sessionId = Guid.NewGuid();
                var accessToken = tokenService.CreateAccessToken(current, sessionId);
                var refreshToken = generatedRefreshToken ?? tokenService.CreateRefreshToken();

                await refreshTokens.AddAsync(
                    RefreshToken.Create(current.Id, refreshToken.TokenHash, refreshToken.ExpiresAt, timeProvider.GetUtcNow().UtcDateTime, ipAddress, sessionId),
                    ct).ConfigureAwait(false);
                await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

                return new AuthResponseDto(accessToken.Token, refreshToken.RawToken, accessToken.ExpiresAt, accessToken.ExpiresInSeconds, current.ToDto());
            },
            cancellationToken).ConfigureAwait(false);
    }
}

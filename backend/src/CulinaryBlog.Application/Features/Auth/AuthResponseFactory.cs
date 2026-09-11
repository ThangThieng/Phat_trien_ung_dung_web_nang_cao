using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>Phát cặp token cho user và lưu refresh token (dùng chung cho Register/Login, sau này Google/Refresh).</summary>
public sealed class AuthResponseFactory(
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<AuthResponseDto> IssueAsync(IdentityUserInfo user, string? ipAddress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = tokenService.CreateRefreshToken();

        await refreshTokens.AddAsync(
            RefreshToken.Create(user.Id, refreshToken.TokenHash, refreshToken.ExpiresAt, timeProvider.GetUtcNow().UtcDateTime, ipAddress),
            cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AuthResponseDto(accessToken.Token, refreshToken.RawToken, accessToken.ExpiresAt, accessToken.ExpiresInSeconds, user.ToDto());
    }
}

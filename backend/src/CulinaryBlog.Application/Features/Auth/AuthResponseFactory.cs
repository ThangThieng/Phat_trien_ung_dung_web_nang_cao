using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions.Auth;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>
/// Phát cặp token cho user — dùng chung cho đăng ký, đăng nhập, Google và refresh, nên MỌI access token đều mang claim
/// <c>sid</c> = Id của bản ghi RefreshToken phát cùng cặp (FR-AUTH-009; Buổi 4). Token phát trước Buổi 4 không có
/// <c>sid</c> và tự hết hạn sau tối đa 15 phút — không cần di trú.
/// </summary>
public sealed class AuthResponseFactory(
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokens,
    IIdentityService identityService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<AuthResponseDto> IssueAsync(IdentityUserInfo user, string? ipAddress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        return await refreshTokens.ExecuteForUserAsync(
            user.Id,
            async ct =>
            {
                // Login/Google may have read the user before a concurrent disable committed.
                var current = await identityService.FindByIdAsync(user.Id, ct).ConfigureAwait(false)
                    ?? throw new InvalidTokenException();
                if (!current.IsActive)
                {
                    throw new AccountDisabledException();
                }

                var refreshToken = tokenService.CreateRefreshToken();
                var session = RefreshToken.Create(current.Id, refreshToken.TokenHash, refreshToken.ExpiresAt, timeProvider.GetUtcNow().UtcDateTime, ipAddress);
                await refreshTokens.AddAsync(session, ct).ConfigureAwait(false);
                await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                return Build(current, session, refreshToken.RawToken);
            },
            cancellationToken).ConfigureAwait(false);
    }

    private AuthResponseDto Build(IdentityUserInfo user, RefreshToken session, string rawRefreshToken)
    {
        var accessToken = tokenService.CreateAccessToken(user, session.Id);
        return new AuthResponseDto(accessToken.Token, rawRefreshToken, accessToken.ExpiresAt, accessToken.ExpiresInSeconds, user.ToDto());
    }
}

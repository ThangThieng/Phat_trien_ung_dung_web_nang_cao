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
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<AuthResponseDto> IssueAsync(IdentityUserInfo user, string? ipAddress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        var refreshToken = tokenService.CreateRefreshToken();
        var session = RefreshToken.Create(user.Id, refreshToken.TokenHash, refreshToken.ExpiresAt, timeProvider.GetUtcNow().UtcDateTime, ipAddress);

        await unitOfWork.Users.AddRefreshTokenAsync(session, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Build(user, session, refreshToken.RawToken);
    }

    /// <summary>
    /// FR-AUTH-004 bước 5–8 — Token Rotation trong MỘT transaction: thu hồi token cũ với
    /// <c>ReplacedByTokenHash = SHA256(token mới)</c> (MT-15, không bao giờ lưu token gốc) và lưu bản ghi token mới.
    /// Thu hồi là UPDATE có điều kiện <c>RevokedAt IS NULL</c>: hai request refresh cùng một token chạy song song thì chỉ
    /// một request xoay vòng được, request kia nhận 401 thay vì rẽ đôi token family.
    /// </summary>
    public async Task<AuthResponseDto> RotateAsync(
        IdentityUserInfo user,
        Guid currentTokenId,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        var refreshToken = tokenService.CreateRefreshToken();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        RefreshToken? session = null;

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                var revoked = await unitOfWork.Users
                    .RevokeRefreshTokenIfActiveAsync(currentTokenId, now, refreshToken.TokenHash, ct)
                    .ConfigureAwait(false);
                if (!revoked)
                {
                    throw InvalidTokenException.RefreshTokenRevoked();
                }

                session = RefreshToken.Create(user.Id, refreshToken.TokenHash, refreshToken.ExpiresAt, now, ipAddress);
                await unitOfWork.Users.AddRefreshTokenAsync(session, ct).ConfigureAwait(false);
                await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);

        return Build(user, session!, refreshToken.RawToken);
    }

    private AuthResponseDto Build(IdentityUserInfo user, RefreshToken session, string rawRefreshToken)
    {
        var accessToken = tokenService.CreateAccessToken(user, session.Id);
        return new AuthResponseDto(accessToken.Token, rawRefreshToken, accessToken.ExpiresAt, accessToken.ExpiresInSeconds, user.ToDto());
    }
}

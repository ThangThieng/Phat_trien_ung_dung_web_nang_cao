using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>
/// FR-AUTH-005 – thu hồi refresh token của phiên hiện tại. Idempotent: token không tồn tại, thuộc người khác hoặc đã
/// thu hồi đều trả 204 — trả 404 sẽ biến endpoint thành "oracle" dò token nào còn hợp lệ.
/// </summary>
public sealed record LogoutCommand(string RefreshToken) : IRequest;

public sealed class LogoutCommandHandler(
    ICurrentUser currentUser,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return;
        }

        await unitOfWork.Users.ExecuteForUserAsync(
            userId,
            async ct =>
            {
                var token = await unitOfWork.Users.GetRefreshTokenByHashAsync(tokenService.HashToken(request.RefreshToken), ct).ConfigureAwait(false);
                if (token is not null && token.UserId == userId)
                {
                    await unitOfWork.Users.RevokeRefreshTokenIfActiveAsync(
                        token.Id,
                        timeProvider.GetUtcNow().UtcDateTime,
                        replacedByTokenHash: null,
                        ct).ConfigureAwait(false);
                }

                return true;
            },
            cancellationToken).ConfigureAwait(false);
    }
}

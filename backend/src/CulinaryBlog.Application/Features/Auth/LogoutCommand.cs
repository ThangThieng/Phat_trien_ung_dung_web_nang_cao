using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>FR-AUTH-005 – thu hồi refresh token của phiên hiện tại, idempotent.</summary>
public sealed record LogoutCommand(string RefreshToken) : IRequest;

public sealed class LogoutCommandHandler(
    ICurrentUser currentUser,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId!;
        var token = await refreshTokens.GetByHashAsync(tokenService.HashToken(request.RefreshToken), cancellationToken).ConfigureAwait(false);
        if (token is not null && token.UserId == userId)
        {
            await refreshTokens.RevokeAsync(token, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

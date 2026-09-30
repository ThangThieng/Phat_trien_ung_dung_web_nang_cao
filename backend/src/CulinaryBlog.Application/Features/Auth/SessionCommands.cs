using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Exceptions.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>SRS §8.1 – một phiên = một refresh token còn hiệu lực. TUYỆT ĐỐI không có TokenHash.</summary>
public sealed record SessionDto(Guid Id, DateTime CreatedAt, string? CreatedByIp, DateTime ExpiresAt, bool IsCurrent);

/// <summary>FR-AUTH-009 – danh sách phiên của chính mình; <c>isCurrent</c> xác định bằng claim <c>sid</c> của access token.</summary>
public sealed record GetMySessionsQuery : IRequest<IReadOnlyList<SessionDto>>;

public sealed class GetMySessionsQueryHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : IRequestHandler<GetMySessionsQuery, IReadOnlyList<SessionDto>>
{
    public async Task<IReadOnlyList<SessionDto>> Handle(GetMySessionsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new InvalidTokenException();
        var tokens = await unitOfWork.Users
            .GetActiveRefreshTokensAsync(userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken)
            .ConfigureAwait(false);

        return [.. tokens.Select(t => new SessionDto(t.Id, t.CreatedAt, t.CreatedByIp, t.ExpiresAt, t.Id == currentUser.SessionId))];
    }
}

/// <summary>
/// FR-AUTH-009 – thu hồi một phiên. Trả <c>false</c> khi phiên không tồn tại HOẶC thuộc người khác — endpoint trả 404
/// cho cả hai (A1) để không dò được id nào đang tồn tại. Phiên đã thu hồi trước đó vẫn là thành công (A2, idempotent).
/// </summary>
public sealed record RevokeSessionCommand(Guid SessionId) : IRequest<bool>;

public sealed class RevokeSessionCommandHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : IRequestHandler<RevokeSessionCommand, bool>
{
    public async Task<bool> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUser.UserId ?? throw new InvalidTokenException();
        var token = await unitOfWork.Users.GetRefreshTokenByIdAsync(userId, request.SessionId, cancellationToken).ConfigureAwait(false);
        if (token is null)
        {
            return false;
        }

        token.Revoke(timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}

/// <summary>FR-AUTH-009 – "đăng xuất trên mọi thiết bị": thu hồi mọi phiên của người gọi, KỂ CẢ phiên hiện tại.</summary>
public sealed record RevokeAllSessionsCommand : IRequest;

public sealed class RevokeAllSessionsCommandHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : IRequestHandler<RevokeAllSessionsCommand>
{
    public async Task Handle(RevokeAllSessionsCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new InvalidTokenException();
        await unitOfWork.Users
            .RevokeAllRefreshTokensAsync(userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken)
            .ConfigureAwait(false);
    }
}

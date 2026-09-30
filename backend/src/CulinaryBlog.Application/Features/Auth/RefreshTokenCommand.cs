using CulinaryBlog.Application.Common.Interfaces.Persistence;
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
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("refreshToken không được để trống.");
    }
}

/// <summary>
/// Cây quyết định (kế hoạch Buổi 4 — Dev 1, SRS FR-AUTH-004):
/// 1. Hash không có trong DB → 401 AUTH_TOKEN_INVALID (A1).
/// 2. Hết hạn → 401 AUTH_REFRESH_TOKEN_EXPIRED (A2).
/// 3. Đã thu hồi → REUSE DETECTED: thu hồi CẢ token family của người dùng, ghi cảnh báo bảo mật, rồi mới ném
///    401 AUTH_REFRESH_TOKEN_REVOKED (A3). Thu hồi phải được ghi xuống DB TRƯỚC khi ném — ném trước thì request kết thúc
///    mà lệnh thu hồi chưa bao giờ tới DB.
/// 4. Người dùng không còn tồn tại → 401 (A5); bị vô hiệu hóa → 403 AUTH_ACCOUNT_DISABLED (A4).
/// 5. Hợp lệ → xoay vòng trong một transaction (AuthResponseFactory.RotateAsync).
/// </summary>
public sealed partial class RefreshTokenCommandHandler(
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    IIdentityService identityService,
    AuthResponseFactory authResponseFactory,
    TimeProvider timeProvider,
    ILogger<RefreshTokenCommandHandler> logger) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Raw token không bao giờ là khóa tra cứu và không bao giờ vào log — chỉ hash SHA-256 của nó.
        var token = await unitOfWork.Users
            .GetRefreshTokenByHashAsync(tokenService.HashToken(request.RefreshToken), cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidTokenException();

        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (token.RevokedAt is not null)
        {
            var revokedCount = await unitOfWork.Users.RevokeAllRefreshTokensAsync(token.UserId, now, cancellationToken).ConfigureAwait(false);
            LogReuseDetected(logger, token.UserId, request.IpAddress, revokedCount);
            throw InvalidTokenException.RefreshTokenRevoked();
        }

        if (token.ExpiresAt <= now)
        {
            throw InvalidTokenException.RefreshTokenExpired();
        }

        var user = await identityService.FindByIdAsync(token.UserId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidTokenException();

        if (!user.IsActive)
        {
            throw new AccountDisabledException();
        }

        return await authResponseFactory.RotateAsync(user, token.Id, request.IpAddress, cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "SECURITY ALERT: refresh token reuse detected for user {UserId} from IP {Ip} – revoked {RevokedCount} active token(s) of the family")]
    private static partial void LogReuseDetected(ILogger logger, string userId, string? ip, int revokedCount);
}

using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Auth;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>FR-AUTH-003 – đăng nhập / đăng ký bằng ID Token do Google Identity Services cấp (ID Token flow, MT-11).</summary>
public sealed record GoogleLoginCommand(string IdToken) : IRequest<AuthResponseDto>
{
    public string? IpAddress { get; init; }
}

public sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        // JWT = header.payload.signature — sai định dạng thì không cần gọi Google (400 VALIDATION_ERROR).
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google ID token không được để trống.")
            .Must(token => token.Count(character => character == '.') == 2)
            .WithMessage("Google ID token không đúng định dạng.");
    }
}

/// <summary>
/// Bảng lỗi FR-AUTH-003: chữ ký/aud/exp sai → 401 (do <see cref="IGoogleIdTokenValidator"/> ném); JWKS lỗi → 502;
/// token thiếu email hoặc email chưa xác minh khi liên kết → 400; tài khoản bị vô hiệu → 403.
/// </summary>
public sealed class GoogleLoginCommandHandler(
    IGoogleIdTokenValidator googleValidator,
    IIdentityService identityService,
    AuthResponseFactory authResponseFactory)
    : IRequestHandler<GoogleLoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var payload = await googleValidator.ValidateAsync(request.IdToken, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new BusinessRuleViolationException(ErrorCodes.AuthGoogleTokenInvalid, "Google ID token không chứa địa chỉ email.");
        }

        var user = await identityService.FindOrCreateGoogleUserAsync(payload, cancellationToken).ConfigureAwait(false);
        if (!user.IsActive)
        {
            throw new AccountDisabledException();
        }

        return await authResponseFactory.IssueAsync(user, request.IpAddress, cancellationToken).ConfigureAwait(false);
    }
}

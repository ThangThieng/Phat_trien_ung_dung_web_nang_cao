using System.Text.Json;
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
        RuleFor(x => x.IdToken)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Google ID token không được để trống.")
            .WithErrorCode(ErrorCodes.AuthGoogleTokenInvalid)
            .Must(IsJwtFormat).WithMessage("Google ID token không đúng định dạng.")
            .WithErrorCode(ErrorCodes.AuthGoogleTokenInvalid);
    }

    private static bool IsJwtFormat(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3 || parts.Any(part => part.Length == 0 ||
            part.Any(character => !((character is >= 'A' and <= 'Z') || (character is >= 'a' and <= 'z') ||
                (character is >= '0' and <= '9') || character is '-' or '_'))))
        {
            return false;
        }

        try
        {
            using var header = JsonDocument.Parse(Decode(parts[0]));
            using var payload = JsonDocument.Parse(Decode(parts[1]));
            return header.RootElement.ValueKind == JsonValueKind.Object &&
                payload.RootElement.ValueKind == JsonValueKind.Object && Decode(parts[2]).Length > 0;
        }
        catch (Exception exception) when (exception is FormatException or JsonException)
        {
            return false;
        }
    }

    private static byte[] Decode(string part)
    {
        var base64 = part.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '='));
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

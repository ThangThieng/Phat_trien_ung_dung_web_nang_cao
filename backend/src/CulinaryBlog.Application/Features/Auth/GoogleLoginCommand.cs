using System.Text.Json;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Exceptions.Auth;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>FR-AUTH-003 – xác thực ID Token do Google Identity Services cấp.</summary>
public sealed record GoogleLoginCommand(string IdToken) : IRequest<AuthResponseDto>
{
    public string? IpAddress { get; init; }
}

public sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google ID token không được để trống.")
            .Must(HasJwtStructure)
            .WithMessage("Google ID token không đúng định dạng.");
    }

    private static bool HasJwtStructure(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var parts = token.Split('.');
        if (parts.Length != 3 || parts.Any(string.IsNullOrWhiteSpace))
        {
            return false;
        }

        try
        {
            using var header = JsonDocument.Parse(System.Buffers.Text.Base64Url.DecodeFromChars(parts[0]));
            using var payload = JsonDocument.Parse(System.Buffers.Text.Base64Url.DecodeFromChars(parts[1]));
            return header.RootElement.ValueKind == JsonValueKind.Object && payload.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return false;
        }
    }
}

public sealed class GoogleLoginCommandHandler(IGoogleIdTokenValidator googleValidator, IIdentityService identityService, AuthResponseFactory authResponseFactory)
    : IRequestHandler<GoogleLoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var identity = await googleValidator.ValidateAsync(request.IdToken, cancellationToken).ConfigureAwait(false);
        if (!identity.EmailVerified || string.IsNullOrWhiteSpace(identity.Email) || string.IsNullOrWhiteSpace(identity.Subject))
        {
            throw new BadRequestException(ErrorCodes.AuthGoogleTokenInvalid, "Google chưa xác minh địa chỉ email của tài khoản này.");
        }

        var user = await identityService.AuthenticateGoogleAsync(identity, cancellationToken).ConfigureAwait(false);
        if (!user.IsActive)
        {
            throw new AccountDisabledException();
        }

        return await authResponseFactory.IssueAsync(user, request.IpAddress, cancellationToken).ConfigureAwait(false);
    }
}

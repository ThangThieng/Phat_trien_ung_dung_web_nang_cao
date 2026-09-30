using CulinaryBlog.Application.Common.Exceptions;
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
            .Must(token => token.Count(character => character == '.') == 2)
            .WithMessage("Google ID token không đúng định dạng.");
    }
}

public sealed class GoogleLoginCommandHandler(IIdentityService identityService, AuthResponseFactory authResponseFactory)
    : IRequestHandler<GoogleLoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.AuthenticateGoogleAsync(request.IdToken, cancellationToken).ConfigureAwait(false);
        if (!user.IsActive)
        {
            throw new ForbiddenException(ErrorCodes.AuthAccountDisabled, "Tài khoản đã bị vô hiệu hóa bởi quản trị viên.");
        }

        return await authResponseFactory.IssueAsync(user, request.IpAddress, cancellationToken).ConfigureAwait(false);
    }
}

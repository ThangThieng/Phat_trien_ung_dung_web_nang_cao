using CulinaryBlog.Domain.Exceptions.Auth;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>FR-AUTH-002 – Đăng nhập bằng Email/Mật khẩu.</summary>
public sealed record LoginUserCommand(string Email, string Password) : IRequest<AuthResponseDto>
{
    public string? IpAddress { get; init; }
}

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(x => x.Password).NotEmpty().WithMessage("Mật khẩu không được để trống.");
    }
}

public sealed class LoginUserCommandHandler(
    IIdentityService identityService,
    AuthResponseFactory authResponseFactory,
    TimeProvider timeProvider)
    : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.CheckPasswordAsync(request.Email.Trim(), request.Password, cancellationToken).ConfigureAwait(false);

        switch (result.Status)
        {
            case PasswordCheckStatus.Success when result.User is not null:
                return await authResponseFactory.IssueAsync(result.User, request.IpAddress, cancellationToken).ConfigureAwait(false);

            case PasswordCheckStatus.Disabled:
                throw new AccountDisabledException();

            case PasswordCheckStatus.LockedOut:
                var remaining = result.LockoutEnd.HasValue ? result.LockoutEnd.Value - timeProvider.GetUtcNow() : TimeSpan.Zero;
                throw new AccountLockedException(result.LockoutEnd, Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes)));

            default:
                // A1: thông báo chung – không tiết lộ email có tồn tại hay không (chống User Enumeration)
                throw new InvalidCredentialsException();
        }
    }
}

using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Exceptions.Auth;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>FR-AUTH-001 – Đăng ký tài khoản mới (auto-login, role Author, welcome email). Body <c>{ email, password, displayName }</c> (D-1).</summary>
public sealed record RegisterUserCommand(string DisplayName, string Email, string Password) : IRequest<AuthResponseDto>
{
    /// <summary>IP client – do endpoint gán, lưu vào RefreshToken.CreatedByIp để audit.</summary>
    public string? IpAddress { get; init; }
}

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        // SRS §7.9: ApplicationUser.DisplayName 2–100 ký tự
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Tên hiển thị không được để trống.")
            .Length(2, 100).WithMessage("Tên hiển thị phải từ 2 đến 100 ký tự.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.")
            .MaximumLength(256);

        RuleFor(x => x.Password).StrongPassword();
    }
}

public sealed class RegisterUserCommandHandler(
    IIdentityService identityService,
    AuthResponseFactory authResponseFactory,
    IWelcomeEmailScheduler welcomeEmailScheduler)
    : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Bước 4: email chưa tồn tại
        if (await identityService.EmailExistsAsync(request.Email, cancellationToken).ConfigureAwait(false))
        {
            throw new EmailAlreadyExistsException();
        }

        // Bước 5–7: tạo user (PBKDF2, UserName do hệ thống sinh – D-1) + gán role Author
        var user = await identityService
            .CreateUserAsync(request.DisplayName.Trim(), request.Email.Trim(), request.Password, Roles.Author, cancellationToken)
            .ConfigureAwait(false);

        // Bước 8–10: access token + refresh token (persist)
        var response = await authResponseFactory.IssueAsync(user, request.IpAddress, cancellationToken).ConfigureAwait(false);

        // Bước 11: welcome email fire-and-forget qua Hangfire
        welcomeEmailScheduler.ScheduleWelcomeEmail(user.Id);

        return response;
    }
}

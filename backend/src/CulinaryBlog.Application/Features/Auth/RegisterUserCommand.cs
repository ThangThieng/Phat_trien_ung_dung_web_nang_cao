using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>FR-AUTH-001 – Đăng ký tài khoản mới (auto-login, role Author, welcome email).</summary>
public sealed record RegisterUserCommand(string DisplayName, string Email, string Password) : IRequest<AuthResponseDto>
{
    /// <summary>IP client – do endpoint gán, lưu vào RefreshToken.CreatedByIp để audit.</summary>
    public string? IpAddress { get; init; }
}

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .Length(2, 100).WithMessage("Họ tên phải từ 2 đến 100 ký tự.");

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
            throw new ConflictException(ErrorCodes.AuthEmailExists, "Email đã được đăng ký bởi tài khoản khác.");
        }

        // Bước 5–7: tạo user (PBKDF2) + gán role Author
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

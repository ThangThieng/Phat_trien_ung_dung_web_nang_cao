using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Auth;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).StrongPassword().NotEqual(x => x.CurrentPassword).WithMessage("Mật khẩu mới phải khác mật khẩu hiện tại.");
    }
}

public sealed partial class ChangePasswordCommandHandler(ICurrentUser currentUser, IIdentityService identityService, ILogger<ChangePasswordCommandHandler> logger)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new InvalidTokenException();
        await identityService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, currentUser.SessionId, cancellationToken).ConfigureAwait(false);
        LogPasswordChanged(logger, userId);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "SECURITY: password changed for user {UserId}; other refresh sessions revoked")]
    private static partial void LogPasswordChanged(ILogger logger, string userId);
}

public sealed record ForgotPasswordCommand(string Email, string CorrelationId) : IRequest;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

public sealed class ForgotPasswordCommandHandler(IIdentityService identityService, IPasswordResetEmailScheduler scheduler) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByEmailAsync(request.Email.Trim(), cancellationToken).ConfigureAwait(false);
        if (user is { IsActive: true })
        {
            scheduler.SchedulePasswordResetEmail(user.Id, request.CorrelationId);
        }
    }
}

public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).StrongPassword();
    }
}

public sealed class ResetPasswordCommandHandler(IIdentityService identityService) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.ResetPasswordAsync(request.Email.Trim(), request.Token, request.NewPassword, cancellationToken).ConfigureAwait(false);
        switch (result)
        {
            case PasswordResetStatus.Success:
                return;
            case PasswordResetStatus.Disabled:
                throw new AccountDisabledException();
            default:
                throw new PasswordResetTokenInvalidException();
        }
    }
}

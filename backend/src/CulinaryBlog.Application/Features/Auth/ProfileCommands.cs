using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth;

public sealed record GetCurrentUserQuery : IRequest<UserProfileDto>;

public sealed class GetCurrentUserQueryHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Yêu cầu đăng nhập.");
        var user = await identityService.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        return user?.ToProfileDto() ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Tài khoản không còn hợp lệ.");
    }
}

public sealed record UpdateProfileCommand(ProfileUpdate Update, string AllowedAvatarBaseUrl) : IRequest<UserProfileDto>;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command.Update.DisplayName)
            .Must(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length is >= 2 and <= 100)
            .When(command => command.Update.HasDisplayName)
            .OverridePropertyName("displayName")
            .WithMessage("Tên hiển thị phải dài từ 2 đến 100 ký tự.");
        RuleFor(command => command.Update.Bio).MaximumLength(1000)
            .When(command => command.Update.HasBio).OverridePropertyName("bio");
        RuleFor(command => command.Update.AvatarUrl)
            .Must((command, url) => url is null || IsAllowedAvatar(url, command.AllowedAvatarBaseUrl))
            .When(command => command.Update.HasAvatarUrl).OverridePropertyName("avatarUrl")
            .WithMessage("Ảnh đại diện phải là URL hợp lệ trong MinIO của Culinary Blog.");
    }

    private static bool IsAllowedAvatar(string value, string allowedBaseUrl)
    {
        if (value.Length > 500 || !Uri.TryCreate(value, UriKind.Absolute, out var avatar) ||
            !Uri.TryCreate($"{allowedBaseUrl.TrimEnd('/')}/", UriKind.Absolute, out var bucket))
        {
            return false;
        }

        // Compare normalized paths: a raw string prefix accepts /bucket/../another-bucket/file.
        return avatar.Scheme is "http" or "https" && avatar.Scheme == bucket.Scheme &&
            string.Equals(avatar.IdnHost, bucket.IdnHost, StringComparison.OrdinalIgnoreCase) &&
            avatar.Port == bucket.Port && string.IsNullOrEmpty(avatar.UserInfo) &&
            avatar.AbsolutePath.StartsWith(bucket.AbsolutePath, StringComparison.Ordinal) &&
            avatar.AbsolutePath.Length > bucket.AbsolutePath.Length &&
            !Uri.UnescapeDataString(avatar.AbsolutePath).Split('/').Any(segment => segment is "." or "..");
    }
}

public sealed class UpdateProfileCommandHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Yêu cầu đăng nhập.");
        try
        {
            return (await identityService.UpdateProfileAsync(userId, request.Update, cancellationToken).ConfigureAwait(false)).ToProfileDto();
        }
        catch (KeyNotFoundException)
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Tài khoản không còn hợp lệ.");
        }
    }
}

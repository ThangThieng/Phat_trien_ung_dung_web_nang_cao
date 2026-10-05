using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using FluentValidation;
using FluentValidation.Results;
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

public sealed class UpdateProfileCommandHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Yêu cầu đăng nhập.");
        var update = request.Update;
        var failures = new List<ValidationFailure>();
        if (update.HasDisplayName && (string.IsNullOrWhiteSpace(update.DisplayName) || update.DisplayName.Trim().Length is < 2 or > 100))
        {
            failures.Add(new ValidationFailure("displayName", "Tên hiển thị phải dài từ 2 đến 100 ký tự."));
        }

        if (update.HasBio && update.Bio?.Length > 1000)
        {
            failures.Add(new ValidationFailure("bio", "Tiểu sử không được vượt quá 1000 ký tự."));
        }

        if (update.HasAvatarUrl && update.AvatarUrl is { } avatarUrl)
        {
            var prefix = $"{request.AllowedAvatarBaseUrl.TrimEnd('/')}/";
            if (avatarUrl.Length > 500 || !Uri.TryCreate(avatarUrl, UriKind.Absolute, out _) ||
                !avatarUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                failures.Add(new ValidationFailure("avatarUrl", "Ảnh đại diện phải là URL hợp lệ trong MinIO của Culinary Blog."));
            }
        }

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        try
        {
            return (await identityService.UpdateProfileAsync(userId, update, cancellationToken).ConfigureAwait(false)).ToProfileDto();
        }
        catch (KeyNotFoundException)
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Tài khoản không còn hợp lệ.");
        }
    }
}

using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Exceptions.Auth;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>
/// FR-AUTH-006 – hồ sơ của người đang đăng nhập (<c>GET /auth/me</c>). Là nguồn sự thật về <c>roles</c> cho Frontend
/// điều hướng các route <c>/dashboard*</c>. Không bao giờ chứa PasswordHash, SecurityStamp, UserName, EmailConfirmed.
/// </summary>
public sealed record GetCurrentUserQuery : IRequest<UserProfileDto>;

public sealed class GetCurrentUserQueryHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // A1 (MT-33.9): token hợp lệ nhưng người dùng không còn tồn tại → 401, không phải 404 — token trỏ tới một danh
        // tính không còn hợp lệ.
        var userId = currentUser.UserId ?? throw new InvalidTokenException();
        var user = await identityService.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidTokenException();
        return user.ToProfileDto();
    }
}

/// <summary>
/// FR-AUTH-007 – PATCH hồ sơ <c>{ displayName?, avatarUrl?, bio? }</c>: trường không gửi thì giữ nguyên; gửi chuỗi rỗng
/// ở avatarUrl/bio để xóa. Email và UserName không đổi được ở đây.
/// </summary>
public sealed record UpdateProfileCommand(string? DisplayName, string? AvatarUrl, string? Bio,
    bool? HasDisplayName = null, bool? HasAvatarUrl = null, bool? HasBio = null) : IRequest<UserProfileDto>;

/// <summary>Giới hạn theo SRS §7.9 (ApplicationUser.DisplayName 2–100, Bio ≤ 1000) và FR-AUTH-007 (avatarUrl ≤ 500).</summary>
public static class ProfileLimits
{
    public const int DisplayNameMin = 2;
    public const int DisplayNameMax = 100;
    public const int BioMax = 1000;
    public const int AvatarUrlMax = 500;
}

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator(IFileStorageService storage)
    {
        RuleFor(x => x.DisplayName!)
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Tên hiển thị không được để trống.")
            .Length(ProfileLimits.DisplayNameMin, ProfileLimits.DisplayNameMax)
                .WithMessage($"Tên hiển thị phải từ {ProfileLimits.DisplayNameMin} đến {ProfileLimits.DisplayNameMax} ký tự.")
            .When(x => x.HasDisplayName ?? x.DisplayName is not null);

        RuleFor(x => x.Bio!)
            .MaximumLength(ProfileLimits.BioMax).WithMessage($"Giới thiệu tối đa {ProfileLimits.BioMax} ký tự.")
            .When(x => (x.HasBio ?? x.Bio is not null) && x.Bio is not null);

        RuleFor(x => x.AvatarUrl!)
            .MaximumLength(ProfileLimits.AvatarUrlMax).WithMessage($"Đường dẫn ảnh đại diện tối đa {ProfileLimits.AvatarUrlMax} ký tự.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("Đường dẫn ảnh đại diện không hợp lệ.")
            .Must(storage.IsStoredFileUrl)
                .WithMessage("Ảnh đại diện phải là tệp đã tải lên hệ thống (POST /api/v1/files/upload).")
            .When(x => (x.HasAvatarUrl ?? x.AvatarUrl is not null) && !string.IsNullOrEmpty(x.AvatarUrl));
    }
}

public sealed class UpdateProfileCommandHandler(ICurrentUser currentUser, IIdentityService identityService)
    : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUser.UserId ?? throw new InvalidTokenException();
        var changes = new ProfileChanges(
            request.DisplayName?.Trim(),
            request.AvatarUrl?.Trim(),
            request.Bio?.Trim(),
            request.HasDisplayName ?? request.DisplayName is not null,
            request.HasAvatarUrl ?? request.AvatarUrl is not null,
            request.HasBio ?? request.Bio is not null);
        var user = await identityService.UpdateProfileAsync(userId, changes, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidTokenException();
        return user.ToProfileDto();
    }
}

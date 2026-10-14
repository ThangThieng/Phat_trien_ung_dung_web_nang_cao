using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// IIdentityService dựa trên ASP.NET Core Identity UserManager:
/// hash mật khẩu PBKDF2 (IPasswordHasher mặc định của Identity – cấu hình 100.000 vòng lặp), lockout 5 lần / 15 phút.
/// Mọi thao tác GHI người dùng của hệ thống đi qua lớp này.
/// </summary>
public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    IUserNameGenerator userNameGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IIdentityService
{
    private const string GoogleProvider = "Google";

    /// <summary>Tên unique index của Identity trên NormalizedUserName.</summary>
    private const string UserNameIndex = "UserNameIndex";

    /// <summary>Mã IdentityError khi UserValidator thấy UserName đã có người dùng.</summary>
    private const string DuplicateUserNameCode = "DuplicateUserName";

    /// <summary>Số lần thử lại khi hai người đăng ký cùng lúc với cùng tiền tố email và cùng nhận một UserName.</summary>
    private const int UserNameAttempts = 3;

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        var normalized = userManager.NormalizeEmail(email);
        return userManager.Users.AnyAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public async Task<IdentityUserInfo> CreateUserAsync(
        string displayName,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken)
    {
        // Tạo user + gán role trong MỘT transaction: lỗi ở bước gán role không để lại user "mồ côi" không có quyền nào.
        var user = await CreateInTransactionAsync(
            email,
            userName => ApplicationUser.Create(displayName, email, userName, timeProvider.GetUtcNow().UtcDateTime),
            password,
            role,
            googleLogin: null,
            cancellationToken).ConfigureAwait(false);

        return ToInfo(user, [role]);
    }

    public async Task<IdentityUserInfo> FindOrCreateGoogleUserAsync(GoogleIdTokenPayload payload, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payload);
        var email = payload.Email ?? throw new ArgumentException("Payload Google phải có email.", nameof(payload));
        var googleLogin = new UserLoginInfo(GoogleProvider, payload.Subject, GoogleProvider);

        // 1. Đã liên kết Google từ trước → đăng nhập.
        var user = await userManager.FindByLoginAsync(GoogleProvider, payload.Subject).ConfigureAwait(false);
        if (user is null)
        {
            user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user is null)
            {
                // 3. Hoàn toàn mới → tạo tài khoản Author (không mật khẩu; chỉ đăng nhập được bằng Google).
                //    CreateAsync → AddToRoleAsync → AddLoginAsync trong MỘT transaction (review Buổi 3, mục 8): lỗi giữa
                //    chừng không để lại user thiếu role hoặc thiếu liên kết Google.
                user = await CreateInTransactionAsync(
                    email,
                    userName =>
                    {
                        var created = ApplicationUser.Create(
                            string.IsNullOrWhiteSpace(payload.Name) ? userName : payload.Name.Trim(),
                            email,
                            userName,
                            timeProvider.GetUtcNow().UtcDateTime);
                        created.AvatarUrl = payload.Picture;
                        created.EmailConfirmed = payload.EmailVerified;
                        return created;
                    },
                    password: null,
                    Roles.Author,
                    googleLogin,
                    cancellationToken).ConfigureAwait(false);
            }
            else if (!payload.EmailVerified)
            {
                // 2. Email đã có tài khoản: chỉ liên kết khi Google xác nhận email — nếu không, kẻ tấn công tạo tài khoản
                //    Google gắn email của nạn nhân là chiếm được tài khoản Culinary Blog của nạn nhân.
                throw new BusinessRuleViolationException(
                    ErrorCodes.AuthGoogleTokenInvalid,
                    "Google chưa xác minh địa chỉ email này nên không thể liên kết với tài khoản đang có.");
            }
            else
            {
                ThrowIfFailed(await userManager.AddLoginAsync(user, googleLogin).ConfigureAwait(false));
            }
        }

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        return ToInfo(user, [.. roles]);
    }

    public async Task<PasswordCheckResult> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (user is null)
        {
            return new PasswordCheckResult(PasswordCheckStatus.InvalidCredentials, null, null);
        }

        if (await userManager.IsLockedOutAsync(user).ConfigureAwait(false))
        {
            return new PasswordCheckResult(PasswordCheckStatus.LockedOut, null, user.LockoutEnd);
        }

        if (!await userManager.CheckPasswordAsync(user, password).ConfigureAwait(false))
        {
            // A3: tăng AccessFailedCount – đạt 5 lần thì Identity tự đặt LockoutEnd = now + 15 phút
            await userManager.AccessFailedAsync(user).ConfigureAwait(false);

            return await userManager.IsLockedOutAsync(user).ConfigureAwait(false)
                ? new PasswordCheckResult(PasswordCheckStatus.LockedOut, null, user.LockoutEnd)
                : new PasswordCheckResult(PasswordCheckStatus.InvalidCredentials, null, null);
        }

        // Chỉ báo "bị vô hiệu hóa" khi mật khẩu đúng – tránh lộ thông tin tài khoản
        if (!user.IsActive)
        {
            return new PasswordCheckResult(PasswordCheckStatus.Disabled, null, null);
        }

        await userManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);
        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        return new PasswordCheckResult(PasswordCheckStatus.Success, ToInfo(user, [.. roles]), null);
    }

    public async Task<IdentityUserInfo?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        return user is null ? null : ToInfo(user, [.. await userManager.GetRolesAsync(user).ConfigureAwait(false)]);
    }

    public async Task<IdentityUserInfo?> UpdateProfileAsync(string userId, ProfileChanges changes, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(changes);

        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null)
        {
            return null;
        }

        if (changes.DisplayName is not null)
        {
            user.DisplayName = changes.DisplayName;
        }

        if (changes.AvatarUrl is not null)
        {
            user.AvatarUrl = changes.AvatarUrl.Length == 0 ? null : changes.AvatarUrl;
        }

        if (changes.Bio is not null)
        {
            user.Bio = changes.Bio.Length == 0 ? null : changes.Bio;
        }

        // UpdateAsync (không ghi thẳng bảng): đổi ConcurrencyStamp để hai lần sửa đồng thời không âm thầm đè nhau.
        ThrowIfFailed(await userManager.UpdateAsync(user).ConfigureAwait(false));
        return ToInfo(user, [.. await userManager.GetRolesAsync(user).ConfigureAwait(false)]);
    }

    public async Task<IdentityUserInfo?> SetActiveAsync(string userId, bool isActive, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null)
        {
            return null;
        }

        if (user.IsActive != isActive)
        {
            user.IsActive = isActive;
            ThrowIfFailed(await userManager.UpdateAsync(user).ConfigureAwait(false));
        }

        return ToInfo(user, [.. await userManager.GetRolesAsync(user).ConfigureAwait(false)]);
    }

    internal static IdentityUserInfo ToInfo(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(user.Id, user.DisplayName, user.Email ?? string.Empty, user.UserName ?? string.Empty, user.AvatarUrl, user.IsActive, roles, user.Bio);

    /// <summary>Chuyển IdentityError thành FluentValidation failures → HTTP 400 VALIDATION_ERROR (FR-AUTH-001 A2, MT-08).</summary>
    internal static void ThrowIfFailed(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var failures = result.Errors.Select(e => new ValidationFailure(MapProperty(e.Code), e.Description) { ErrorCode = e.Code });
        throw new ValidationException(failures);
    }

    /// <summary>
    /// Tạo user (+ role, + liên kết Google nếu có) trong một transaction qua IUnitOfWork (execution strategy của DbContext).
    /// UserName do <see cref="IUserNameGenerator"/> sinh; hai người đăng ký cùng lúc với cùng tiền tố có thể cùng nhận một
    /// tên (review Buổi 3, mục 10) — khi đó thử lại với tên kế tiếp thay vì trả 400 khó hiểu cho người dùng:
    /// • UserValidator phát hiện trước khi ghi (IdentityError DuplicateUserName) → thử lại ngay trong transaction;
    /// • hai INSERT đua nhau, unique index UserNameIndex chặn (23505) → transaction đã hỏng, chạy lại cả transaction.
    /// </summary>
    private async Task<ApplicationUser> CreateInTransactionAsync(
        string email,
        Func<string, ApplicationUser> build,
        string? password,
        string role,
        UserLoginInfo? googleLogin,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= UserNameAttempts; attempt++)
        {
            try
            {
                ApplicationUser? created = null;
                await unitOfWork.ExecuteInTransactionAsync(
                    async ct =>
                    {
                        created = await CreateWithFreshUserNameAsync(email, build, password, ct).ConfigureAwait(false);
                        ThrowIfFailed(await userManager.AddToRoleAsync(created, role).ConfigureAwait(false));
                        if (googleLogin is not null)
                        {
                            ThrowIfFailed(await userManager.AddLoginAsync(created, googleLogin).ConfigureAwait(false));
                        }
                    },
                    cancellationToken).ConfigureAwait(false);

                return created!;
            }
            catch (DbUpdateException ex) when (attempt < UserNameAttempts && IsUserNameRace(ex))
            {
                // Người kia vừa lấy mất UserName này — chạy lại, bộ sinh sẽ thấy tên đó đã có chủ.
            }
        }

        throw new InvalidOperationException("Không tạo được người dùng sau nhiều lần thử lại UserName.");
    }

    private async Task<ApplicationUser> CreateWithFreshUserNameAsync(
        string email,
        Func<string, ApplicationUser> build,
        string? password,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= UserNameAttempts; attempt++)
        {
            var user = build(await userNameGenerator.GenerateAsync(email, cancellationToken).ConfigureAwait(false));

            // UserManager.CreateAsync → IPasswordHasher<ApplicationUser> (PBKDF2-HMAC-SHA512)
            var result = password is null
                ? await userManager.CreateAsync(user).ConfigureAwait(false)
                : await userManager.CreateAsync(user, password).ConfigureAwait(false);

            if (result.Succeeded)
            {
                return user;
            }

            if (attempt >= UserNameAttempts || !result.Errors.Any(e => e.Code == DuplicateUserNameCode))
            {
                ThrowIfFailed(result);
            }
        }

        throw new InvalidOperationException("Không tạo được người dùng sau nhiều lần thử lại UserName.");
    }

    private static bool IsUserNameRace(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: UserNameIndex };

    private static string MapProperty(string identityErrorCode) => identityErrorCode switch
    {
        _ when identityErrorCode.StartsWith("Password", StringComparison.Ordinal) => "password",
        _ when identityErrorCode.Contains("Email", StringComparison.Ordinal) => "email",
        _ when identityErrorCode.Contains("UserName", StringComparison.Ordinal) => "userName",
        _ => string.Empty,
    };
}

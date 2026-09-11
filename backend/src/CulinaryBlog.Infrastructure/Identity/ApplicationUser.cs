using Microsoft.AspNetCore.Identity;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// SRS §7.7 – kế thừa IdentityUser (bảng "AspNetUsers") + custom columns.
/// Đặt ở Infrastructure vì phụ thuộc ASP.NET Core Identity (Domain không được có NuGet dependency – CONS-001).
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Tên hiển thị công khai (FullName trong API).</summary>
    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    /// <summary>Admin có thể vô hiệu hóa tài khoản (ban).</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public static ApplicationUser Create(string fullName, string email, string userName, DateTime createdAt) =>
        new()
        {
            DisplayName = fullName,
            Email = email,
            UserName = userName,
            IsActive = true,
            CreatedAt = createdAt,
        };
}

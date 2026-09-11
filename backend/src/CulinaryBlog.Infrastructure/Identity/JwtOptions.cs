using System.ComponentModel.DataAnnotations;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>Cấu hình JWT (CONS-004, NFR-SEC-002). SigningKey lấy từ User Secrets / biến môi trường.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "CulinaryBlog";

    [Required]
    public string Audience { get; set; } = "CulinaryBlog.Client";

    /// <summary>Khóa HS256 – tối thiểu 32 byte (256-bit).</summary>
    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 60)]
    public int AccessTokenMinutes { get; set; } = 15;

    [Range(1, 30)]
    public int RefreshTokenDays { get; set; } = 7;
}

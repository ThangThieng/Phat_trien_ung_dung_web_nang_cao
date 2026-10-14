namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>Cấu hình Google Identity Services cho luồng ID-token (FR-AUTH-003).</summary>
public sealed class GoogleAuthOptions
{
    public const string SectionName = "Authentication:Google";

    public string ClientId { get; set; } = string.Empty;
}

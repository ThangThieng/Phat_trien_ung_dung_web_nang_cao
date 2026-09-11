namespace CulinaryBlog.Domain.Entities;

/// <summary>SRS §7.8 – refresh token (chỉ lưu SHA-256 hash, không lưu raw token).</summary>
public class RefreshToken
{
    private RefreshToken()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

    public string UserId { get; private set; } = string.Empty;

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public string? CreatedByIp { get; private set; }

    public static RefreshToken Create(string userId, string tokenHash, DateTime expiresAt, DateTime createdAt, string? createdByIp) =>
        new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = createdAt,
            CreatedByIp = createdByIp,
        };

    public bool IsActive(DateTime utcNow) => RevokedAt is null && ExpiresAt > utcNow;
}

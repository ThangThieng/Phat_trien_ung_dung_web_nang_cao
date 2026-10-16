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

    /// <summary>
    /// Thu hồi token. Idempotent: token đã thu hồi giữ nguyên <see cref="RevokedAt"/> và <see cref="ReplacedByTokenHash"/>
    /// ban đầu (dấu vết của token family không bị ghi đè). <paramref name="replacedByTokenHash"/> là SHA-256 của token
    /// mới khi xoay vòng (FR-AUTH-004 bước 6, MT-15) — không bao giờ là token gốc.
    /// </summary>
    public void Revoke(DateTime utcNow, string? replacedByTokenHash = null)
    {
        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = utcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}

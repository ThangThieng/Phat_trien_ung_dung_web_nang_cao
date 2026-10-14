using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Common.Authorization;
using CulinaryBlog.Application.Features.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// ITokenService – Access Token: JWT HS256, TTL 15 phút, claims sub/email/name/roles/jti + sid (NFR-SEC-002, FR-AUTH-009).
/// Refresh Token: 256-bit cryptographically secure random (MT-13, retrofit D-2 Buổi 4), Base64URL; DB chỉ lưu SHA-256 hex —
/// 256-bit khớp đúng độ dài đầu ra SHA-256 và cột TokenHash varchar(64); entropy cao hơn không tăng an toàn thực chất vì
/// đằng nào cũng bị hash, chỉ tốn băng thông mỗi lần refresh.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    private const int RefreshTokenBytes = 32;

    private readonly JwtOptions _options = options.Value;

    public AccessToken CreateAccessToken(IdentityUserInfo user, Guid? sessionId = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        claims.AddRange(user.Roles.Select(role => new Claim(AppClaimTypes.Role, role)));
        if (sessionId is { } sid)
        {
            claims.Add(new Claim(AppClaimTypes.SessionId, sid.ToString()));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
                SecurityAlgorithms.HmacSha256),
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new AccessToken(token, expires, _options.AccessTokenMinutes * 60);
    }

    public GeneratedRefreshToken CreateRefreshToken()
    {
        var raw = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(RefreshTokenBytes));
        var expires = timeProvider.GetUtcNow().UtcDateTime.AddDays(_options.RefreshTokenDays);
        return new GeneratedRefreshToken(raw, HashToken(raw), expires);
    }

    public string HashToken(string rawToken) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}

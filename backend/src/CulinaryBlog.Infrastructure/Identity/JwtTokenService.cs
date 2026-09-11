using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Features.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// ITokenService – Access Token: JWT HS256, TTL 15 phút, claims userId/email/roles/jti (NFR-SEC-002).
/// Refresh Token: 512-bit cryptographically secure random (FR-AUTH-001 bước 9), lưu DB dạng SHA-256 hex.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    private const int RefreshTokenBytes = 64;

    private readonly JwtOptions _options = options.Value;

    public AccessToken CreateAccessToken(IdentityUserInfo user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        claims.AddRange(user.Roles.Select(role => new Claim("role", role)));

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

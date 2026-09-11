using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CulinaryBlog.API.Services;

/// <summary>ICurrentUser từ JWT claims của HttpContext (claim "sub" = UserId, "role" = roles).</summary>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? UserId => IsAuthenticated ? accessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value : null;

    public bool IsAuthenticated => accessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsAdmin => accessor.HttpContext?.User.IsInRole(Roles.Admin) == true;

    public string? IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}

using System.Security.Claims;
using CulinaryBlog.Application.Common.Authorization;
using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.API.Services;

/// <summary>ICurrentUser từ JWT claims của HttpContext (claim "sub" = UserId, "role" = roles).</summary>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? UserId => IsAuthenticated ? Principal.FindFirst(AppClaimTypes.Subject)?.Value : null;

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public bool IsAdmin => Principal.IsInRole(Roles.Admin);

    /// <summary>IP thật của client — đã qua UseForwardedHeaders khi request đi sau Nginx.</summary>
    public string? IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public ClaimsPrincipal Principal => accessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
}

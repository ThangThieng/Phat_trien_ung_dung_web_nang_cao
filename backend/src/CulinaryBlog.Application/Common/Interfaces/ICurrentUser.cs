using System.Security.Claims;

namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>Người dùng của request hiện tại (lấy từ JWT claims).</summary>
public interface ICurrentUser
{
    string? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }

    string? IpAddress { get; }

    /// <summary>Claims của request — đầu vào cho resource-based authorization (<c>IAuthorizationService.AuthorizeAsync</c>).</summary>
    ClaimsPrincipal Principal { get; }
}

public static class Roles
{
    public const string Author = "Author";
    public const string Admin = "Admin";
}

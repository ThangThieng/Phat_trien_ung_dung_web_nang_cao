namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>Người dùng của request hiện tại (lấy từ JWT claims).</summary>
public interface ICurrentUser
{
    string? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }

    string? IpAddress { get; }
}

public static class Roles
{
    public const string Author = "Author";
    public const string Admin = "Admin";
}

namespace CulinaryBlog.Application.Common.Authorization;

/// <summary>
/// Tên claim trong access token của hệ thống (JWT, <c>MapInboundClaims = false</c> nên giữ nguyên tên ngắn).
/// Một nguồn duy nhất cho nơi phát token (Infrastructure) và nơi đọc token (API, handler phân quyền).
/// </summary>
public static class AppClaimTypes
{
    /// <summary>UserId (AspNetUsers.Id).</summary>
    public const string Subject = "sub";

    public const string Role = "role";

    /// <summary>FR-AUTH-009: Id của bản ghi RefreshToken phát cùng cặp token — xác định "phiên hiện tại".</summary>
    public const string SessionId = "sid";
}

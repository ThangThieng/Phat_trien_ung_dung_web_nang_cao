using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

/// <summary>SRS §8.1 – /api/v1/users: quản lý tài khoản của Admin (FR-AUTH-008). Mọi endpoint yêu cầu AdminPolicy.</summary>
public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapGet("/", GetUsersAsync)
            .WithName("GetUsers")
            .WithSummary("FR-AUTH-008 – Danh sách người dùng (phân trang, tìm theo email/tên, lọc isActive)")
            .Produces<PagedResult<UserAdminDto>>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPatch("/{id}/status", SetStatusAsync)
            .WithName("SetUserStatus")
            .WithSummary("FR-AUTH-008 – Khóa / mở khóa tài khoản (khóa thì thu hồi mọi phiên)")
            .Produces<UserStatusDto>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return api;
    }

    /// <summary>Dữ liệu quản trị → <c>Cache-Control: no-store</c> (SRS §8 quy ước Cache, NFR-SEC-006).</summary>
    private static async Task<IResult> GetUsersAsync(
        HttpContext http,
        ISender sender,
        CancellationToken ct,
        int page = 1,
        int pageSize = 12,
        string? search = null,
        bool? isActive = null)
    {
        var result = await sender.Send(new GetUsersQuery(page, pageSize, search, isActive), ct).ConfigureAwait(false);
        http.Response.Headers.CacheControl = "no-store";
        return Results.Ok(result);
    }

    private static async Task<IResult> SetStatusAsync(string id, SetUserStatusRequest body, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new SetUserStatusCommand(id, body.IsActive, body.Reason), ct).ConfigureAwait(false));

    public sealed record SetUserStatusRequest(bool? IsActive, string? Reason);
}

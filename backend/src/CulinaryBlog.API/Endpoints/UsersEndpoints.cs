using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

/// <summary>FR-AUTH-008 – quản trị danh sách và trạng thái tài khoản.</summary>
public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/users").WithTags("Users").RequireAuthorization(AuthorizationPolicies.Admin);
        group.MapGet(string.Empty, GetUsersAsync)
            .WithName("GetUsers")
            .WithSummary("FR-AUTH-008 – Tìm kiếm người dùng (Admin)")
            .Produces<CulinaryBlog.Application.Common.Models.PagedResult<UserAdminDto>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);
        group.MapPatch("/{id}/status", SetStatusAsync)
            .WithName("SetUserStatus")
            .WithSummary("FR-AUTH-008 – Khóa hoặc mở khóa tài khoản (Admin)")
            .Produces<UserStatusDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
        return api;
    }

    private static async Task<IResult> GetUsersAsync(
        int? page, int? pageSize, string? search, bool? isActive, HttpContext http, ISender sender, CancellationToken ct)
    {
        http.Response.Headers.CacheControl = "no-store";
        var query = new GetUsersQuery(page ?? 1, pageSize ?? 12, search, isActive);
        return Results.Ok(await sender.Send(query, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> SetStatusAsync(string id, UserStatusRequest body, ISender sender, CancellationToken ct)
    {
        if (!body.IsActive.HasValue)
        {
            throw new BadRequestException(ErrorCodes.ValidationError, "Trường isActive là bắt buộc.");
        }

        return Results.Ok(await sender.Send(new SetUserStatusCommand(id, body.IsActive.Value, body.Reason), ct).ConfigureAwait(false));
    }

    public sealed record UserStatusRequest(bool? IsActive, string? Reason);
}

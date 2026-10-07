using System.Text.Json;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

/// <summary>SRS §8.1 – /api/v1/auth.</summary>
public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("FR-AUTH-001 – Đăng ký tài khoản (role Author, tự động đăng nhập)")
            .Produces<AuthResponseDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("FR-AUTH-002 – Đăng nhập Email/Mật khẩu")
            .Produces<AuthResponseDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status423Locked)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/google", GoogleLoginAsync)
            .WithName("GoogleLogin")
            .WithSummary("FR-AUTH-003 – Đăng nhập hoặc đăng ký bằng Google ID Token")
            .Produces<AuthResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .WithSummary("FR-AUTH-005 – Thu hồi refresh token của phiên hiện tại")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/refresh", RefreshAsync)
            .WithName("Refresh")
            .WithSummary("FR-AUTH-004 – Refresh token rotation và reuse detection")
            .Produces<AuthResponseDto>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/me", GetMeAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithSummary("FR-AUTH-006 – Lấy hồ sơ người dùng hiện tại")
            .Produces<UserProfileDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPatch("/me", PatchMeAsync)
            .RequireAuthorization()
            .WithName("UpdateCurrentUser")
            .WithSummary("FR-AUTH-007 – Cập nhật hồ sơ cá nhân")
            .Produces<UserProfileDto>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/sessions", GetSessionsAsync)
            .RequireAuthorization()
            .WithName("GetMySessions")
            .WithSummary("FR-AUTH-009 – Liệt kê phiên đăng nhập còn hiệu lực")
            .Produces<IReadOnlyList<SessionDto>>()
            .WithMetadata(new Microsoft.AspNetCore.Mvc.ResponseCacheAttribute { NoStore = true, Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.None });

        group.MapDelete("/sessions/{id:guid}", RevokeSessionAsync)
            .RequireAuthorization()
            .WithName("RevokeMySession")
            .WithSummary("FR-AUTH-009 – Thu hồi một phiên đăng nhập")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/sessions/revoke-all", RevokeAllSessionsAsync)
            .RequireAuthorization()
            .WithName("RevokeAllMySessions")
            .WithSummary("FR-AUTH-009 – Thu hồi tất cả phiên đăng nhập")
            .Produces(StatusCodes.Status204NoContent);

        return api;
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var command = new RegisterUserCommand(body.DisplayName ?? string.Empty, body.Email ?? string.Empty, body.Password ?? string.Empty)
        {
            IpAddress = http.Connection.RemoteIpAddress?.ToString(),
        };

        var response = await sender.Send(command, ct).ConfigureAwait(false);
        return Results.Created("/api/v1/auth/me", response);
    }

    private static async Task<IResult> LoginAsync(LoginRequest body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var command = new LoginUserCommand(body.Email ?? string.Empty, body.Password ?? string.Empty)
        {
            IpAddress = http.Connection.RemoteIpAddress?.ToString(),
        };

        return Results.Ok(await sender.Send(command, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> GoogleLoginAsync(GoogleLoginRequest? body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var command = new GoogleLoginCommand(body?.IdToken ?? string.Empty)
        {
            IpAddress = http.Connection.RemoteIpAddress?.ToString(),
        };
        return Results.Ok(await sender.Send(command, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> LogoutAsync(LogoutRequest body, ISender sender, CancellationToken ct)
    {
        await sender.Send(new LogoutCommand(body.RefreshToken ?? string.Empty), ct).ConfigureAwait(false);
        return Results.NoContent();
    }

    private static async Task<IResult> RefreshAsync(RefreshRequest? body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var command = new RefreshTokenCommand(body?.RefreshToken ?? string.Empty, http.Connection.RemoteIpAddress?.ToString());
        return Results.Ok(await sender.Send(command, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> GetMeAsync(HttpContext http, ISender sender, CancellationToken ct)
    {
        http.Response.Headers.CacheControl = "no-store";
        return Results.Ok(await sender.Send(new GetCurrentUserQuery(), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> PatchMeAsync(ProfilePatchRequest body, HttpContext http, IConfiguration configuration, ISender sender, CancellationToken ct)
    {
        http.Response.Headers.CacheControl = "no-store";
        var profile = new ProfileUpdate(
            GetString(body.DisplayName, "displayName", out var hasDisplayName),
            hasDisplayName,
            GetString(body.AvatarUrl, "avatarUrl", out var hasAvatarUrl),
            hasAvatarUrl,
            GetString(body.Bio, "bio", out var hasBio),
            hasBio);
        var baseUrl = configuration["MinIO:PublicBaseUrl"] ?? string.Empty;
        return Results.Ok(await sender.Send(new UpdateProfileCommand(profile, baseUrl), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> GetSessionsAsync(HttpContext http, ISender sender, CancellationToken ct)
    {
        http.Response.Headers.CacheControl = "no-store";
        return Results.Ok(await sender.Send(new GetMySessionsQuery(), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> RevokeSessionAsync(Guid id, ISender sender, CancellationToken ct)
    {
        await sender.Send(new RevokeMySessionCommand(id), ct).ConfigureAwait(false);
        return Results.NoContent();
    }

    private static async Task<IResult> RevokeAllSessionsAsync(ISender sender, CancellationToken ct)
    {
        await sender.Send(new RevokeAllMySessionsCommand(), ct).ConfigureAwait(false);
        return Results.NoContent();
    }

    private static string? GetString(JsonElement value, string propertyName, out bool provided)
    {
        provided = value.ValueKind != JsonValueKind.Undefined;
        if (!provided || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        throw new BadRequestException(ErrorCodes.ValidationError, $"Trường {propertyName} phải là chuỗi hoặc null.");
    }

    public sealed record RegisterRequest(string? DisplayName, string? Email, string? Password);

    public sealed record LoginRequest(string? Email, string? Password);

    public sealed record GoogleLoginRequest(string? IdToken);

    public sealed record LogoutRequest(string? RefreshToken);

    public sealed record RefreshRequest(string? RefreshToken);

    public sealed record ProfilePatchRequest(JsonElement DisplayName, JsonElement AvatarUrl, JsonElement Bio);
}

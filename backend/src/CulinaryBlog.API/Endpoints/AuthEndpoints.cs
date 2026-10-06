using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

/// <summary>SRS §8.1 – /api/v1/auth: đăng ký/đăng nhập, phiên (refresh, logout, sessions) và hồ sơ của chính mình.</summary>
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
            .ProducesProblem(StatusCodes.Status403Forbidden)
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

        // Không RequireAuthorization: access token đã hết hạn thì không xác thực được — refresh token là bằng chứng.
        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous()
            .WithName("RefreshToken")
            .WithSummary("FR-AUTH-004 – Làm mới access token (Token Rotation + Reuse Detection)")
            .Produces<AuthResponseDto>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .WithSummary("FR-AUTH-005 – Thu hồi refresh token của phiên hiện tại")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", GetMeAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithSummary("FR-AUTH-006 – Hồ sơ của người đang đăng nhập")
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPatch("/me", UpdateMeAsync)
            .RequireAuthorization()
            .WithName("UpdateCurrentUser")
            .WithSummary("FR-AUTH-007 – Cập nhật hồ sơ (displayName, avatarUrl, bio)")
            .Produces<UserDto>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/sessions", GetSessionsAsync)
            .RequireAuthorization()
            .WithName("GetMySessions")
            .WithSummary("FR-AUTH-009 – Danh sách phiên đăng nhập của chính mình")
            .Produces<IReadOnlyList<SessionDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapDelete("/sessions/{id:guid}", RevokeSessionAsync)
            .RequireAuthorization()
            .WithName("RevokeSession")
            .WithSummary("FR-AUTH-009 – Thu hồi một phiên (404 nếu không tồn tại hoặc thuộc người khác)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/sessions/revoke-all", RevokeAllSessionsAsync)
            .RequireAuthorization()
            .WithName("RevokeAllSessions")
            .WithSummary("FR-AUTH-009 – Đăng xuất trên mọi thiết bị (kể cả phiên hiện tại)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return api;
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var command = new RegisterUserCommand(body.DisplayName ?? string.Empty, body.Email ?? string.Empty, body.Password ?? string.Empty)
        {
            IpAddress = ClientIp(http),
        };

        var response = await sender.Send(command, ct).ConfigureAwait(false);
        return Results.Created("/api/v1/auth/me", response);
    }

    private static async Task<IResult> LoginAsync(LoginRequest body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var command = new LoginUserCommand(body.Email ?? string.Empty, body.Password ?? string.Empty)
        {
            IpAddress = ClientIp(http),
        };

        return Results.Ok(await sender.Send(command, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> GoogleLoginAsync(GoogleLoginRequest body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var command = new GoogleLoginCommand(body.IdToken ?? string.Empty)
        {
            IpAddress = ClientIp(http),
        };
        return Results.Ok(await sender.Send(command, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> RefreshAsync(RefreshRequest body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var command = new RefreshTokenCommand(body.RefreshToken ?? string.Empty)
        {
            IpAddress = ClientIp(http),
        };
        return Results.Ok(await sender.Send(command, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> LogoutAsync(LogoutRequest body, ISender sender, CancellationToken ct)
    {
        await sender.Send(new LogoutCommand(body.RefreshToken ?? string.Empty), ct).ConfigureAwait(false);
        return Results.NoContent();
    }

    /// <summary>FR-AUTH-006: phụ thuộc danh tính → không cache (NFR-SEC-006).</summary>
    private static async Task<IResult> GetMeAsync(HttpContext http, ISender sender, CancellationToken ct)
    {
        var user = await sender.Send(new GetCurrentUserQuery(), ct).ConfigureAwait(false);
        http.Response.Headers.CacheControl = "no-store";
        return Results.Ok(user);
    }

    private static async Task<IResult> UpdateMeAsync(UpdateProfileRequest body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var user = await sender.Send(new UpdateProfileCommand(body.DisplayName, body.AvatarUrl, body.Bio), ct).ConfigureAwait(false);
        http.Response.Headers.CacheControl = "no-store";
        return Results.Ok(user);
    }

    /// <summary>SRS §8 quy ước Cache: danh sách phiên phụ thuộc danh tính → <c>Cache-Control: no-store</c>.</summary>
    private static async Task<IResult> GetSessionsAsync(HttpContext http, ISender sender, CancellationToken ct)
    {
        var sessions = await sender.Send(new GetMySessionsQuery(), ct).ConfigureAwait(false);
        http.Response.Headers.CacheControl = "no-store";
        return Results.Ok(sessions);
    }

    private static async Task<IResult> RevokeSessionAsync(Guid id, ISender sender, CancellationToken ct) =>
        await sender.Send(new RevokeSessionCommand(id), ct).ConfigureAwait(false)
            ? Results.NoContent()
            : Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                type: HttpProblemTypes.NotFound,
                title: "Not Found",
                detail: "Không tìm thấy phiên đăng nhập.");

    private static async Task<IResult> RevokeAllSessionsAsync(ISender sender, CancellationToken ct)
    {
        await sender.Send(new RevokeAllSessionsCommand(), ct).ConfigureAwait(false);
        return Results.NoContent();
    }

    /// <summary>IP thật của client: sau <c>UseForwardedHeaders</c>, RemoteIpAddress là IP trong X-Forwarded-For do Nginx gắn.</summary>
    private static string? ClientIp(HttpContext http) => http.Connection.RemoteIpAddress?.ToString();

    public sealed record RegisterRequest(string? DisplayName, string? Email, string? Password);

    public sealed record LoginRequest(string? Email, string? Password);

    public sealed record GoogleLoginRequest(string? IdToken);

    public sealed record RefreshRequest(string? RefreshToken);

    public sealed record LogoutRequest(string? RefreshToken);

    public sealed record UpdateProfileRequest(string? DisplayName, string? AvatarUrl, string? Bio);
}

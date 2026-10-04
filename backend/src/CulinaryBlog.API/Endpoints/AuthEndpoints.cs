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

    private static async Task<IResult> GoogleLoginAsync(GoogleLoginRequest body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var command = new GoogleLoginCommand(body.IdToken ?? string.Empty)
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

    public sealed record RegisterRequest(string? DisplayName, string? Email, string? Password);

    public sealed record LoginRequest(string? Email, string? Password);

    public sealed record GoogleLoginRequest(string? IdToken);

    public sealed record LogoutRequest(string? RefreshToken);
}

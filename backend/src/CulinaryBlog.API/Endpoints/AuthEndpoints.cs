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

        return api;
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var command = new RegisterUserCommand(body.FullName ?? string.Empty, body.Email ?? string.Empty, body.UserName ?? string.Empty, body.Password ?? string.Empty)
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

    public sealed record RegisterRequest(string? FullName, string? Email, string? UserName, string? Password);

    public sealed record LoginRequest(string? Email, string? Password);
}

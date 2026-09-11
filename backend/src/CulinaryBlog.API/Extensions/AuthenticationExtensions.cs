using System.Text;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.API.Extensions;

public static class AuthorizationPolicies
{
    /// <summary>NFR-SEC-006: policy thay vì hardcode role string ở endpoint.</summary>
    public const string Author = "AuthorPolicy";
    public const string Admin = "AdminPolicy";
}

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = "role",
                    ClockSkew = TimeSpan.FromSeconds(30),
                };

                // 401 dạng RFC 7807 với mã AUTH_TOKEN_EXPIRED / AUTH_TOKEN_INVALID (frontend dựa vào đây để refresh – FR-AUTH-004)
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        var expired = context.AuthenticateFailure is SecurityTokenExpiredException;
                        var problems = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await problems.WriteAsync(new ProblemDetailsContext
                        {
                            HttpContext = context.HttpContext,
                            ProblemDetails = new ProblemDetails
                            {
                                Type = expired ? ErrorCodes.AuthTokenExpired : ErrorCodes.AuthTokenInvalid,
                                Title = "Unauthorized",
                                Status = StatusCodes.Status401Unauthorized,
                                Detail = expired ? "Access Token đã hết hạn." : "Access Token thiếu hoặc không hợp lệ.",
                            },
                        }).ConfigureAwait(false);
                    },
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.Author, p => p.RequireAuthenticatedUser().RequireRole(Roles.Author, Roles.Admin))
            .AddPolicy(AuthorizationPolicies.Admin, p => p.RequireAuthenticatedUser().RequireRole(Roles.Admin));

        return services;
    }
}

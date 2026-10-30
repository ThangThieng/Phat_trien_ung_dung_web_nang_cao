using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.API.IntegrationTests.Infrastructure.Fakes;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Jobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Auth;

/// <summary>Integration contracts for the password lifecycle (FR-AUTH-009/011).</summary>
[Collection(IntegrationTestSuite.Name)]

public sealed class PasswordEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_Returns400WithCurrentPasswordField()
    {
        var client = factory.CreateClient();
        var auth = await AuthApi.RegisterAsync(client);
        client.Authorized(auth.AccessToken);

        var response = await client.PostAsJsonAsync("/api/v1/auth/change-password", new
        {
            currentPassword = "Wrong#12345", newPassword = "NewPassword#12345",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("currentPassword", (await response.ReadValidationErrorsAsync()).Keys, StringComparer.Ordinal);
    }

    [Fact]
    public async Task ChangePassword_PreservesCurrentRefreshSessionAndRevokesOtherSessions()
    {
        var client = factory.CreateClient();
        var email = $"password-{Guid.NewGuid():N}@culinaryblog.test";
        var current = await AuthApi.RegisterAsync(client, email);
        var other = await AuthApi.LoginAsync(client, email);
        client.Authorized(current.AccessToken);

        var response = await client.PostAsJsonAsync("/api/v1/auth/change-password", new
        {
            currentPassword = TestDataSeeder.ValidPassword, newPassword = "NewPassword#12345",
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AuthApi.RefreshAsync(client, other.RefreshToken)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await AuthApi.RefreshAsync(client, current.RefreshToken)).StatusCode);
        var oldLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = TestDataSeeder.ValidPassword });
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_GoogleOnlyAccount_Returns400()
    {
        var client = factory.CreateClient();
        var token = factory.GoogleTokens.Issue(new GoogleIdTokenPayload($"google-{Guid.NewGuid():N}", $"google-{Guid.NewGuid():N}@culinaryblog.test", true, "Google User", null));
        var auth = await (await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = token })).ReadAsAsync<AuthResponseDto>();
        client.Authorized(auth.AccessToken);

        var response = await client.PostAsJsonAsync("/api/v1/auth/change-password", new
        {
            currentPassword = "AnyPassword#12345", newPassword = "NewPassword#12345",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("currentPassword", (await response.ReadValidationErrorsAsync()).Keys, StringComparer.Ordinal);
    }

    [Fact]
    public async Task ForgotPassword_AlwaysReturns202_AndOnlyKnownAccountEnqueuesSafeJobArguments()
    {
        var client = factory.CreateClient();
        var auth = await AuthApi.RegisterAsync(client);
        factory.BackgroundJobs.Clear();

        var unknown = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = "unknown@culinaryblog.test" });
        Assert.Equal(HttpStatusCode.Accepted, unknown.StatusCode);
        Assert.DoesNotContain(factory.BackgroundJobs.EnqueuedJobs, job => job.JobType == typeof(PasswordResetEmailJob));

        client.DefaultRequestHeaders.Add("X-Correlation-ID", "correlation-123");
        var known = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = auth.User.Email });
        Assert.Equal(HttpStatusCode.Accepted, known.StatusCode);
        var job = Assert.Single(factory.BackgroundJobs.EnqueuedJobs, job => job.JobType == typeof(PasswordResetEmailJob));
        Assert.Equal(nameof(PasswordResetEmailJob.ExecuteAsync), job.MethodName);
        Assert.Equal(auth.User.Id, job.Arguments[0]);
        Assert.Equal("correlation-123", job.Arguments[1]);
        Assert.Equal(2, job.Arguments.Count);
    }

    [Theory]
    [InlineData("invalid!characters")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task ForgotPassword_InvalidCorrelationHeader_FallsBackToServerTrace(string correlationId)
    {
        var client = factory.CreateClient();
        var auth = await AuthApi.RegisterAsync(client);
        factory.BackgroundJobs.Clear();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

        Assert.Equal(HttpStatusCode.Accepted, (await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = auth.User.Email })).StatusCode);
        var job = Assert.Single(factory.BackgroundJobs.EnqueuedJobs, candidate => candidate.JobType == typeof(PasswordResetEmailJob));
        Assert.NotEqual(correlationId, job.Arguments[1]);
        Assert.False(string.IsNullOrWhiteSpace(Assert.IsType<string>(job.Arguments[1])));
    }

    [Fact]
    public async Task ResetPassword_RealEmailToken_SucceedsRevokesSessionsAndCannotBeReused()
    {
        var client = factory.CreateClient();
        var email = $"reset-{Guid.NewGuid():N}@culinaryblog.test";
        var current = await AuthApi.RegisterAsync(client, email);
        var other = await AuthApi.LoginAsync(client, email);
        using (var scope = factory.Services.CreateScope())
        {
            var job = scope.ServiceProvider.GetRequiredService<PasswordResetEmailJob>();
            await job.ExecuteAsync(current.User.Id, "reset-test");
        }

        var message = Assert.Single(factory.Emails.Sent);
        var token = ExtractToken(message.HtmlBody);
        const string newPassword = "NewPassword#12345";
        var reset = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new { email, token, newPassword });
        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AuthApi.RefreshAsync(client, current.RefreshToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AuthApi.RefreshAsync(client, other.RefreshToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = TestDataSeeder.ValidPassword })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = newPassword })).StatusCode);
        await (await client.PostAsJsonAsync("/api/v1/auth/reset-password", new { email, token, newPassword })).ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.AuthPasswordResetTokenInvalid);
    }

    [Theory]
    [InlineData("invalid-token", "known@culinaryblog.test")]
    [InlineData("invalid-token", "unknown@culinaryblog.test")]
    public async Task ResetPassword_InvalidOrUnknownToken_ReturnsUniform400(string token, string email)
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            email,
            token,
            newPassword = "NewPassword#12345",
        });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.AuthPasswordResetTokenInvalid);
    }

    [Fact]
    public async Task ResetPassword_DisabledAccount_Returns403()
    {
        var client = factory.CreateClient();
        var auth = await AuthApi.RegisterAsync(client);
        using (var scope = factory.Services.CreateScope())
        {
            var job = scope.ServiceProvider.GetRequiredService<PasswordResetEmailJob>();
            await job.ExecuteAsync(auth.User.Id, "disabled-test");
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByIdAsync(auth.User.Id);
            Assert.NotNull(user);
            user.IsActive = false;
            Assert.True((await users.UpdateAsync(user)).Succeeded);
        }

        var token = ExtractToken(Assert.Single(factory.Emails.Sent).HtmlBody);
        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            email = auth.User.Email,
            token,
            newPassword = "NewPassword#12345",
        });

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.AuthAccountDisabled);
    }

    [Fact]
    public async Task ResetPassword_Success_ResetsAccessFailedCount()
    {
        var client = factory.CreateClient();
        var auth = await AuthApi.RegisterAsync(client);
        for (var attempt = 0; attempt < 2; attempt++)
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/v1/auth/login", new { email = auth.User.Email, password = "WrongPassword#12345" })).StatusCode);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.True((await users.FindByIdAsync(auth.User.Id))!.AccessFailedCount > 0);
            await scope.ServiceProvider.GetRequiredService<PasswordResetEmailJob>().ExecuteAsync(auth.User.Id, "failed-count-test");
        }

        var token = ExtractToken(Assert.Single(factory.Emails.Sent).HtmlBody);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/v1/auth/reset-password", new { email = auth.User.Email, token, newPassword = "NewPassword#12345" })).StatusCode);
        using var verificationScope = factory.Services.CreateScope();
        var verificationUsers = verificationScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Equal(0, (await verificationUsers.FindByIdAsync(auth.User.Id))!.AccessFailedCount);
    }

    [Fact]
    public async Task ResetPassword_ExpiredToken_ReturnsInvalidToken()
    {
        using var shortLived = factory.WithWebHostBuilder(builder => builder.UseSetting("Identity:PasswordResetTokenLifespanMinutes", "0"));
        var client = shortLived.CreateClient();
        var auth = await AuthApi.RegisterAsync(client);
        factory.Emails.Clear();
        using (var scope = shortLived.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<PasswordResetEmailJob>().ExecuteAsync(auth.User.Id, "expiry-test");
        }

        var token = ExtractToken(Assert.Single(factory.Emails.Sent).HtmlBody);
        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            email = auth.User.Email,
            token,
            newPassword = "NewPassword#12345",
        });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.AuthPasswordResetTokenInvalid);
    }

    private static string ExtractToken(string html)
    {
        var match = Regex.Match(System.Net.WebUtility.HtmlDecode(html), @"[?&]token=([^&\""<]+)");
        Assert.True(match.Success, "The reset email must contain a reset token link.");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }
}

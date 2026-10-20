using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions.Auth;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests;

[Collection(IntegrationTestSuite.Name)]
public sealed class TokenRevocationConcurrencyTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RevocationCannotMissConcurrentReplacement(bool disableAccount)
    {
        using var setup = factory.CreateClient();
        var original = await RegisterAsync(setup).ConfigureAwait(true);
        var other = await RegisterAsync(setup).ConfigureAwait(true);
        var actor = disableAccount ? await RegisterAsync(setup, admin: true).ConfigureAwait(true) : original;
        var gate = new RevocationGate(original.User.Id);
        using var gatedFactory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddScoped<IUserRepository>(provider =>
            {
                var db = provider.GetRequiredService<CulinaryBlogDbContext>();
                return new GatedRepository(new UserRepository(db), db, gate);
            })));
        using var actorClient = gatedFactory.CreateClient();
        using var refreshClient = gatedFactory.CreateClient();
        actorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", actor.AccessToken);
        var revocation = disableAccount
            ? actorClient.PatchAsJsonAsync($"/api/v1/users/{original.User.Id}/status", new { isActive = false })
            : actorClient.PostAsync("/api/v1/auth/sessions/revoke-all", null);
        try
        {
            await gate.SnapshotRead.Task.WaitAsync(TimeSpan.FromSeconds(20)).ConfigureAwait(true);
            var refresh = refreshClient.PostAsJsonAsync("/api/v1/auth/refresh", new { original.RefreshToken });
            var pid = await gate.RefreshPid.Task.WaitAsync(TimeSpan.FromSeconds(20)).ConfigureAwait(true);

            // Before the fix refresh commits R2 while revocation holds its old snapshot.
            // After the fix PostgreSQL reports refresh waiting on the revoker's user-row lock.
            await WaitForCompletionOrDatabaseLockAsync(refresh, pid).ConfigureAwait(true);
            gate.Release.TrySetResult();
            using var revokeResponse = await revocation.ConfigureAwait(true);
            using var refreshResponse = await refresh.ConfigureAwait(true);
            Assert.Equal(disableAccount ? HttpStatusCode.OK : HttpStatusCode.NoContent, revokeResponse.StatusCode);
            await using var scope = factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            Assert.False(await db.RefreshTokens.AnyAsync(token => token.UserId == original.User.Id && token.RevokedAt == null).ConfigureAwait(true));
            Assert.Equal(!disableAccount, await db.Users.Where(user => user.Id == original.User.Id).Select(user => user.IsActive).SingleAsync().ConfigureAwait(true));
            await AssertRefreshErrorAsync(refreshResponse, disableAccount).ConfigureAwait(true);
            using var later = await setup.PostAsJsonAsync("/api/v1/auth/refresh", new { original.RefreshToken }).ConfigureAwait(true);
            await AssertRefreshErrorAsync(later, disableAccount).ConfigureAwait(true);
            using var unaffected = await setup.PostAsJsonAsync("/api/v1/auth/refresh", new { other.RefreshToken }).ConfigureAwait(true);
            Assert.Equal(HttpStatusCode.OK, unaffected.StatusCode);
        }
        finally
        {
            gate.Release.TrySetResult();
            using var cleanup = await revocation.ConfigureAwait(true);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RotationWinningLockStillHasItsReplacementRevoked(bool disableAccount)
    {
        using var setup = factory.CreateClient();
        var original = await RegisterAsync(setup).ConfigureAwait(true);
        var other = await RegisterAsync(setup).ConfigureAwait(true);
        var actor = disableAccount ? await RegisterAsync(setup, admin: true).ConfigureAwait(true) : original;
        var gate = new RevocationGate(original.User.Id) { PauseRotation = true };
        using var gatedFactory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddScoped<IUserRepository>(provider =>
            {
                var db = provider.GetRequiredService<CulinaryBlogDbContext>();
                return new GatedRepository(new UserRepository(db), db, gate);
            })));
        using var client = gatedFactory.CreateClient();
        using var actorClient = gatedFactory.CreateClient();
        actorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", actor.AccessToken);
        var refresh = client.PostAsJsonAsync("/api/v1/auth/refresh", new { original.RefreshToken });
        try
        {
            await gate.RotationPersisted.Task.WaitAsync(TimeSpan.FromSeconds(20)).ConfigureAwait(true);
            var pid = await gate.RefreshPid.Task.ConfigureAwait(true);
            var revoke = disableAccount
                ? actorClient.PatchAsJsonAsync($"/api/v1/users/{original.User.Id}/status", new { isActive = false })
                : actorClient.PostAsync("/api/v1/auth/sessions/revoke-all", null);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            await using var scope = factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            while (await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM pg_stat_activity WHERE {pid} = ANY(pg_blocking_pids(pid))")
                .SingleAsync(timeout.Token).ConfigureAwait(true) == 0)
            {
                timeout.Token.ThrowIfCancellationRequested();
            }

            gate.Release.TrySetResult();
            using var refreshResponse = await refresh.ConfigureAwait(true);
            Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
            var replacement = (await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
            using var revokeResponse = await revoke.ConfigureAwait(true);
            Assert.Equal(disableAccount ? HttpStatusCode.OK : HttpStatusCode.NoContent, revokeResponse.StatusCode);
            Assert.False(await db.RefreshTokens.AnyAsync(token => token.UserId == original.User.Id && token.RevokedAt == null).ConfigureAwait(true));
            using var later = await setup.PostAsJsonAsync("/api/v1/auth/refresh", new { replacement.RefreshToken }).ConfigureAwait(true);
            await AssertRefreshErrorAsync(later, disableAccount).ConfigureAwait(true);
            using var unaffected = await setup.PostAsJsonAsync("/api/v1/auth/refresh", new { other.RefreshToken }).ConfigureAwait(true);
            Assert.Equal(HttpStatusCode.OK, unaffected.StatusCode);
        }
        finally
        {
            gate.Release.TrySetResult();
            using var cleanup = await refresh.ConfigureAwait(true);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FailedRevocationRollsBackUserAndTokens(bool disableAccount)
    {
        using var setup = factory.CreateClient();
        var original = await RegisterAsync(setup).ConfigureAwait(true);
        var actor = disableAccount ? await RegisterAsync(setup, admin: true).ConfigureAwait(true) : original;
        var gate = new RevocationGate(original.User.Id) { FailAfterSnapshot = true };
        using var failingFactory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddScoped<IUserRepository>(provider =>
            {
                var db = provider.GetRequiredService<CulinaryBlogDbContext>();
                return new GatedRepository(new UserRepository(db), db, gate);
            })));
        using var client = failingFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", actor.AccessToken);
        using var response = disableAccount
            ? await client.PatchAsJsonAsync($"/api/v1/users/{original.User.Id}/status", new { isActive = false }).ConfigureAwait(true)
            : await client.PostAsync("/api/v1/auth/sessions/revoke-all", null).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.True(await db.Users.Where(user => user.Id == original.User.Id).Select(user => user.IsActive).SingleAsync().ConfigureAwait(true));
        Assert.Null((await db.RefreshTokens.SingleAsync(token => token.UserId == original.User.Id).ConfigureAwait(true)).RevokedAt);
        using var refresh = await setup.PostAsJsonAsync("/api/v1/auth/refresh", new { original.RefreshToken }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
    }

    [Fact]
    public async Task IssuanceRechecksDisabledUserEvenWithStaleTrackedIdentity()
    {
        using var client = factory.CreateClient();
        var original = await RegisterAsync(client).ConfigureAwait(true);
        await using var issuingScope = factory.Services.CreateAsyncScope();
        var db = issuingScope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        await db.Users.SingleAsync(user => user.Id == original.User.Id).ConfigureAwait(true);
        var stale = Assert.IsType<IdentityUserInfo>(await issuingScope.ServiceProvider.GetRequiredService<IIdentityService>().FindByIdAsync(original.User.Id, CancellationToken.None).ConfigureAwait(true));
        await using (var disablingScope = factory.Services.CreateAsyncScope())
        {
            await disablingScope.ServiceProvider.GetRequiredService<IIdentityService>().SetActiveAsync(original.User.Id, false, CancellationToken.None).ConfigureAwait(true);
        }

        await Assert.ThrowsAsync<AccountDisabledException>(() => issuingScope.ServiceProvider.GetRequiredService<AuthResponseFactory>()
            .IssueAsync(stale, "203.0.113.42", CancellationToken.None)).ConfigureAwait(true);
        Assert.Equal(1, await db.RefreshTokens.CountAsync(token => token.UserId == original.User.Id).ConfigureAwait(true));
        Assert.False(await db.RefreshTokens.AnyAsync(token => token.UserId == original.User.Id && token.RevokedAt == null).ConfigureAwait(true));
    }

    private async Task WaitForCompletionOrDatabaseLockAsync(Task<HttpResponseMessage> refresh, int pid)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        while (!refresh.IsCompleted)
        {
            var blocked = await db.Database.SqlQuery<int>($"SELECT cardinality(pg_blocking_pids({pid})) AS \"Value\"")
                .SingleAsync(timeout.Token).ConfigureAwait(true);
            if (blocked > 0)
            {
                return;
            }
        }
    }

    private static async Task AssertRefreshErrorAsync(HttpResponseMessage response, bool disabled)
    {
        Assert.Equal(disabled ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(true));
        Assert.Equal(disabled ? "AUTH_ACCOUNT_DISABLED" : "AUTH_REFRESH_TOKEN_REVOKED", json.RootElement.GetProperty("type").GetString());
    }

    private async Task<AuthResponseDto> RegisterAsync(HttpClient client, bool admin = false)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            displayName = "Revocation race", email = $"race-{Guid.NewGuid():N}@example.com", password = "TestPassw0rd!",
        }).ConfigureAwait(true);
        response.EnsureSuccessStatusCode();
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
        if (admin)
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roles.RoleExistsAsync("Admin").ConfigureAwait(true))
            {
                Assert.True((await roles.CreateAsync(new IdentityRole("Admin")).ConfigureAwait(true)).Succeeded);
            }

            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = (await users.FindByIdAsync(auth.User.Id).ConfigureAwait(true))!;
            Assert.True((await users.AddToRoleAsync(user, "Admin").ConfigureAwait(true)).Succeeded);
            using var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { auth.User.Email, password = "TestPassw0rd!" }).ConfigureAwait(true);
            login.EnsureSuccessStatusCode();
            auth = (await login.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
        }

        return auth;
    }

    private sealed class RevocationGate(string userId)
    {
        public string UserId { get; } = userId;

        public bool PauseRotation { get; init; }

        public bool FailAfterSnapshot { get; init; }

        public TaskCompletionSource RotationPersisted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource SnapshotRead { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<int> RefreshPid { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class GatedRepository(IUserRepository inner, CulinaryBlogDbContext db, RevocationGate gate) : IUserRepository
    {
        public Task<T> ExecuteForUserAsync<T>(string userId, Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default) => inner.ExecuteForUserAsync(userId, operation, cancellationToken);

        public Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default) => inner.AddRefreshTokenAsync(token, cancellationToken);

        public async Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            await db.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            var token = await inner.GetRefreshTokenByHashAsync(tokenHash, cancellationToken).ConfigureAwait(false);
            if (token?.UserId == gate.UserId)
            {
                var pid = await db.Database.SqlQueryRaw<int>("SELECT pg_backend_pid() AS \"Value\"").SingleAsync(cancellationToken).ConfigureAwait(false);
                gate.RefreshPid.TrySetResult(pid);
            }

            return token;
        }

        public async Task<bool> RevokeRefreshTokenIfActiveAsync(Guid tokenId, DateTime utcNow, string? replacedByTokenHash, CancellationToken cancellationToken = default)
        {
            var rotated = await inner.RevokeRefreshTokenIfActiveAsync(tokenId, utcNow, replacedByTokenHash, cancellationToken).ConfigureAwait(false);
            if (rotated && gate.PauseRotation && replacedByTokenHash is not null)
            {
                gate.RotationPersisted.TrySetResult();
                await gate.Release.Task.WaitAsync(TimeSpan.FromSeconds(20), cancellationToken).ConfigureAwait(false);
            }

            return rotated;
        }

        public async Task<int> RevokeAllRefreshTokensAsync(string userId, DateTime utcNow, CancellationToken cancellationToken = default)
        {
            var count = await inner.RevokeAllRefreshTokensAsync(userId, utcNow, cancellationToken).ConfigureAwait(false);
            if (userId == gate.UserId)
            {
                if (gate.FailAfterSnapshot)
                {
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    throw new InvalidOperationException("Injected revocation failure.");
                }

                if (gate.PauseRotation)
                {
                    return count;
                }

                gate.SnapshotRead.TrySetResult();
                await gate.Release.Task.WaitAsync(TimeSpan.FromSeconds(20), cancellationToken).ConfigureAwait(false);
            }

            return count;
        }

        public Task RevokeRefreshTokenFamilyAsync(string userId, string startingTokenHash, DateTime utcNow, CancellationToken cancellationToken = default) => inner.RevokeRefreshTokenFamilyAsync(userId, startingTokenHash, utcNow, cancellationToken);

        public Task<IReadOnlyList<RefreshToken>> GetActiveRefreshTokensAsync(string userId, DateTime utcNow, CancellationToken cancellationToken = default) => inner.GetActiveRefreshTokensAsync(userId, utcNow, cancellationToken);

        public Task<RefreshToken?> GetRefreshTokenAsync(string userId, string tokenHash, CancellationToken cancellationToken = default) => inner.GetRefreshTokenAsync(userId, tokenHash, cancellationToken);

        public Task<RefreshToken?> GetRefreshTokenByIdAsync(string userId, Guid tokenId, CancellationToken cancellationToken = default) => inner.GetRefreshTokenByIdAsync(userId, tokenId, cancellationToken);

        public Task<IReadOnlyList<string>> GetUserNamesStartingWithAsync(string prefix, CancellationToken cancellationToken = default) => inner.GetUserNamesStartingWithAsync(prefix, cancellationToken);

        public Task<PagedResult<UserAdminDto>> GetUsersPageAsync(UserListCriteria criteria, CancellationToken cancellationToken = default) => inner.GetUsersPageAsync(criteria, cancellationToken);
    }
}

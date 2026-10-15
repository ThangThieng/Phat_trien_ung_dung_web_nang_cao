using CulinaryBlog.API.Middleware.ExceptionMapping;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions.Auth;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CulinaryBlog.API.IntegrationTests;

public sealed class Buoi3AuthContractTests
{
    [Theory]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    [InlineData("a.b.c")]
    public void GoogleCommandRejectsMissingOrMalformedJwt(string token)
    {
        var result = new GoogleLoginCommandValidator().Validate(new GoogleLoginCommand(token));
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task UserNameGeneratorUsesNormalizedPrefixAndCollisionSuffixes()
    {
        var repository = new UserNamesProbe();
        var generator = new UserNameGenerator(repository, new UpperInvariantLookupNormalizer());
        Assert.Equal("an.nguyen", await generator.GenerateAsync("An.Nguyen@example.com", CancellationToken.None).ConfigureAwait(true));
        repository.Names.Add("AN.NGUYEN");
        Assert.Equal("an.nguyen2", await generator.GenerateAsync("an.nguyen@other.example", CancellationToken.None).ConfigureAwait(true));
        repository.Names.Add("AN.NGUYEN2");
        Assert.Equal("an.nguyen3", await generator.GenerateAsync("an.nguyen@third.example", CancellationToken.None).ConfigureAwait(true));
        Assert.All(repository.Prefixes, prefix => Assert.Equal("AN.NGUYEN", prefix));
    }

    [Theory]
    [InlineData("credentials", 401, "AUTH_INVALID_CREDENTIALS")]
    [InlineData("token", 401, "AUTH_TOKEN_INVALID")]
    [InlineData("disabled", 403, "AUTH_ACCOUNT_DISABLED")]
    [InlineData("missing-user", 404, "AUTH_USER_NOT_FOUND")]
    [InlineData("email", 409, "AUTH_EMAIL_EXISTS")]
    [InlineData("locked", 423, "AUTH_ACCOUNT_LOCKED")]
    public void AuthExceptionsMapToExpectedProblemDetails(string scenario, int status, string code)
    {
        AuthDomainException exception = scenario switch
        {
            "credentials" => new InvalidCredentialsException(),
            "token" => new InvalidTokenException(),
            "disabled" => new AccountDisabledException(),
            "missing-user" => new UserNotFoundException(),
            "email" => new EmailAlreadyExistsException(),
            "locked" => new AccountLockedException(DateTimeOffset.UtcNow.AddMinutes(15), 15),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };

        var problem = AuthExceptionMappings.ToProblem(exception, "/api/v1/auth/login");
        Assert.Equal(status, problem.Status);
        Assert.Equal(code, problem.Type);
        if (scenario == "locked")
        {
            Assert.NotNull(problem.Extensions["lockoutEnd"]);
            Assert.Equal(15, problem.Extensions["retryAfterMinutes"]);
        }
    }

    private sealed class UserNamesProbe : IUserRepository
    {
        public HashSet<string> Names { get; } = new(StringComparer.Ordinal);

        public List<string> Prefixes { get; } = [];

        public Task<IReadOnlyList<string>> GetNormalizedUserNamesByPrefixAsync(string normalizedPrefix, CancellationToken cancellationToken)
        {
            Prefixes.Add(normalizedPrefix);
            return Task.FromResult<IReadOnlyList<string>>(Names.Where(name => name.StartsWith(normalizedPrefix, StringComparison.Ordinal)).ToArray());
        }

        public Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<RefreshToken?> FindRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task RevokeRefreshTokenAsync(RefreshToken token, DateTime revokedAt, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}

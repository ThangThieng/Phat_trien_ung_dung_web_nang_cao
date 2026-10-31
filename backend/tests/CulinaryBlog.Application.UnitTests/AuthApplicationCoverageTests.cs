using System.Security.Claims;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Auth;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

#pragma warning disable SA1107 // Arrange test doubles compactly around a single assertion.
#pragma warning disable SA1122 // Empty input is intentional validator coverage.
#pragma warning disable SA1516 // Related test helpers remain visually grouped.

namespace CulinaryBlog.Application.UnitTests;

/// <summary>
/// Behaviour-focused coverage for every Auth Application command/query/validator.
/// Infrastructure integration and HTTP mapping remain covered by API integration tests.
/// </summary>
public sealed class AuthApplicationCoverageTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 10, 0, 0, TimeSpan.Zero);
    private static readonly TimeProvider Clock = new FixedTimeProvider(Now);

    [Theory]
    [InlineData("a@b.com", "Password1!", true)]
    [InlineData("", "Password1!", false)]
    [InlineData("not-email", "Password1!", false)]
    [InlineData("a@b.com", "", false)]
    public void Login_validator_enforces_email_and_password(string email, string password, bool valid) =>
        Assert.Equal(valid, new LoginUserCommandValidator().Validate(new LoginUserCommand(email, password)).IsValid);

    [Theory]
    [InlineData("Jane", "jane@example.com", "Password1!", true)]
    [InlineData("J", "jane@example.com", "Password1!", false)]
    [InlineData("Jane", "invalid", "Password1!", false)]
    [InlineData("Jane", "jane@example.com", "short", false)]
    public void Register_validator_enforces_srs_limits(string name, string email, string password, bool valid) =>
        Assert.Equal(valid, new RegisterUserCommandValidator().Validate(new RegisterUserCommand(name, email, password)).IsValid);

    [Theory]
    [InlineData("eyJhbGciOiJub25lIn0.eyJ4IjoxfQ.sig", true)]
    [InlineData("", false)]
    [InlineData("not-a-jwt", false)]
    [InlineData("a..c", false)]
    public void Google_validator_requires_well_formed_jwt(string token, bool valid) =>
        Assert.Equal(valid, new GoogleLoginCommandValidator().Validate(new GoogleLoginCommand(token)).IsValid);

    [Fact]
    public void Refresh_validator_uses_refresh_token_contract()
    {
        Assert.True(new RefreshTokenCommandValidator().Validate(new RefreshTokenCommand("opaque-token")).IsValid);
        Assert.False(new RefreshTokenCommandValidator().Validate(new RefreshTokenCommand("")).IsValid);
    }

    [Theory]
    [InlineData("Password1!", "NewPassword1!", true)]
    [InlineData("Password1!", "Password1!", false)]
    [InlineData("", "NewPassword1!", false)]
    [InlineData("Password1!", "weak", false)]
    public void Change_password_validator_requires_distinct_strong_passwords(string current, string next, bool valid) =>
        Assert.Equal(valid, new ChangePasswordCommandValidator().Validate(new ChangePasswordCommand(current, next)).IsValid);

    [Theory]
    [InlineData("a@example.com", true)]
    [InlineData("not-email", false)]
    [InlineData("", false)]
    public void Forgot_password_validator_is_email_safe(string email, bool valid) =>
        Assert.Equal(valid, new ForgotPasswordCommandValidator().Validate(new ForgotPasswordCommand(email, "corr")).IsValid);

    [Theory]
    [InlineData("a@example.com", "token", "Password1!", true)]
    [InlineData("a@example.com", "", "Password1!", false)]
    [InlineData("a@example.com", "token", "weak", false)]
    public void Reset_password_validator_requires_token_and_strong_password(string email, string token, string password, bool valid) =>
        Assert.Equal(valid, new ResetPasswordCommandValidator().Validate(new ResetPasswordCommand(email, token, password)).IsValid);

    [Theory]
    [InlineData(1, 12, true)]
    [InlineData(0, 12, false)]
    [InlineData(1, 51, false)]
    public void User_list_validator_enforces_pagination(int page, int pageSize, bool valid) =>
        Assert.Equal(valid, new GetUsersQueryValidator().Validate(new GetUsersQuery(page, pageSize)).IsValid);

    [Theory]
    [InlineData(true, "valid reason", true)]
    [InlineData(null, "valid reason", false)]
    [InlineData(false, "bad", false)]
    public void User_status_validator_requires_explicit_status_and_reason(bool? active, string reason, bool valid) =>
        Assert.Equal(valid, new SetUserStatusCommandValidator().Validate(new SetUserStatusCommand("target", active, reason)).IsValid);

    [Fact]
    public void Profile_validator_accepts_owned_avatar_and_rejects_external_url()
    {
        var storage = Substitute.For<IFileStorageService>();
        storage.IsStoredFileUrl("https://cdn.example/avatar.png").Returns(true);
        var validator = new UpdateProfileCommandValidator(storage);

        Assert.True(validator.Validate(new UpdateProfileCommand(" Jane ", "https://cdn.example/avatar.png", " bio ")).IsValid);
        Assert.False(validator.Validate(new UpdateProfileCommand(null, "https://third-party.example/avatar.png", null)).IsValid);
        Assert.False(validator.Validate(new UpdateProfileCommand(" ", null, null)).IsValid);
    }

    [Fact]
    public async Task Auth_response_factory_persists_session_and_uses_sid_access_token()
    {
        var identity = Substitute.For<IIdentityService>();
        var tokens = Substitute.For<ITokenService>();
        var users = Substitute.For<IUserRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        uow.Users.Returns(users);
        var user = User();
        identity.FindByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        tokens.CreateRefreshToken().Returns(new GeneratedRefreshToken("raw-refresh", "hashed-refresh", Now.AddDays(7).UtcDateTime));
        tokens.CreateAccessToken(Arg.Any<IdentityUserInfo>(), Arg.Any<Guid?>()).Returns(new AccessToken("jwt", Now.AddMinutes(15).UtcDateTime, 900));
        users.ExecuteForUserAsync<AuthResponseDto>(Arg.Any<string>(), Arg.Any<Func<CancellationToken, Task<AuthResponseDto>>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<Func<CancellationToken, Task<AuthResponseDto>>>(1)(CancellationToken.None));
        uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var response = await new AuthResponseFactory(tokens, identity, uow, Clock).IssueAsync(user, "127.0.0.1", CancellationToken.None);

        Assert.Equal("jwt", response.AccessToken);
        Assert.Equal("raw-refresh", response.RefreshToken);
        await users.Received(1).AddRefreshTokenAsync(Arg.Is<RefreshToken>(x => x.UserId == user.Id && x.TokenHash == "hashed-refresh" && x.CreatedByIp == "127.0.0.1"), Arg.Any<CancellationToken>());
        tokens.Received(1).CreateAccessToken(user, Arg.Any<Guid?>());
    }

    [Fact]
    public async Task Auth_response_factory_rejects_disabled_user_rechecked_inside_lock()
    {
        var identity = Substitute.For<IIdentityService>();
        var tokens = Substitute.For<ITokenService>();
        var users = Substitute.For<IUserRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        uow.Users.Returns(users);
        var user = User(active: false);
        identity.FindByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        users.ExecuteForUserAsync<AuthResponseDto>(Arg.Any<string>(), Arg.Any<Func<CancellationToken, Task<AuthResponseDto>>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<Func<CancellationToken, Task<AuthResponseDto>>>(1)(CancellationToken.None));

        await Assert.ThrowsAsync<AccountDisabledException>(() => new AuthResponseFactory(tokens, identity, uow, Clock).IssueAsync(user, null, CancellationToken.None));
        await users.DidNotReceive().AddRefreshTokenAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_success_issues_common_auth_response_and_trims_email()
    {
        var identity = Substitute.For<IIdentityService>();
        var factory = Factory(identity, out var tokens, out var users, out var uow);
        var user = User();
        identity.CheckPasswordAsync("jane@example.com", "Password1!", Arg.Any<CancellationToken>()).Returns(new PasswordCheckResult(PasswordCheckStatus.Success, user, null));
        ArrangeIssue(identity, tokens, users, uow, user);

        var result = await new LoginUserCommandHandler(identity, factory, Clock).Handle(new LoginUserCommand(" jane@example.com ", "Password1!") { IpAddress = "10.0.0.1" }, CancellationToken.None);

        Assert.Equal("raw-refresh", result.RefreshToken);
    }

    [Theory]
    [InlineData(PasswordCheckStatus.InvalidCredentials)]
    [InlineData(PasswordCheckStatus.Success)]
    public async Task Login_invalid_or_missing_user_has_generic_failure(PasswordCheckStatus status)
    {
        var identity = Substitute.For<IIdentityService>();
        identity.CheckPasswordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new PasswordCheckResult(status, null, null));
        var handler = new LoginUserCommandHandler(identity, Factory(identity, out _, out _, out _), Clock);
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => handler.Handle(new LoginUserCommand("a@b.com", "Password1!"), CancellationToken.None));
    }

    [Fact]
    public async Task Login_handles_disabled_and_lockout_states()
    {
        var identity = Substitute.For<IIdentityService>();
        var handler = new LoginUserCommandHandler(identity, Factory(identity, out _, out _, out _), Clock);
        identity.CheckPasswordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new PasswordCheckResult(PasswordCheckStatus.Disabled, null, null));
        await Assert.ThrowsAsync<AccountDisabledException>(() => handler.Handle(new LoginUserCommand("a@b.com", "Password1!"), CancellationToken.None));

        identity.CheckPasswordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new PasswordCheckResult(PasswordCheckStatus.LockedOut, null, Now.AddMinutes(4)));
        var error = await Assert.ThrowsAsync<AccountLockedException>(() => handler.Handle(new LoginUserCommand("a@b.com", "Password1!"), CancellationToken.None));
        Assert.Equal(4, error.RetryAfterMinutes);
    }

    [Fact]
    public async Task Register_creates_author_issues_response_and_schedules_welcome_email()
    {
        var identity = Substitute.For<IIdentityService>();
        var scheduler = Substitute.For<IWelcomeEmailScheduler>();
        var factory = Factory(identity, out var tokens, out var users, out var uow);
        var user = User();
        identity.EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        identity.CreateUserAsync("Jane", "jane@example.com", "Password1!", Roles.Author, Arg.Any<CancellationToken>()).Returns(user);
        ArrangeIssue(identity, tokens, users, uow, user);

        await new RegisterUserCommandHandler(identity, factory, scheduler).Handle(new RegisterUserCommand(" Jane ", " jane@example.com ", "Password1!"), CancellationToken.None);

        scheduler.Received(1).ScheduleWelcomeEmail(user.Id);
    }

    [Fact]
    public async Task Register_rejects_existing_email_before_creating_user()
    {
        var identity = Substitute.For<IIdentityService>();
        identity.EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var handler = new RegisterUserCommandHandler(identity, Factory(identity, out _, out _, out _), Substitute.For<IWelcomeEmailScheduler>());
        await Assert.ThrowsAsync<EmailAlreadyExistsException>(() => handler.Handle(new RegisterUserCommand("Jane", "a@b.com", "Password1!"), CancellationToken.None));
        await identity.DidNotReceive().CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Google_login_rejects_missing_email_and_disabled_user()
    {
        var google = Substitute.For<IGoogleIdTokenValidator>();
        var identity = Substitute.For<IIdentityService>();
        var handler = new GoogleLoginCommandHandler(google, identity, Factory(identity, out _, out _, out _));
        google.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new GoogleIdTokenPayload("sub", null, true, null, null));
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.Handle(new GoogleLoginCommand("a.b.c"), CancellationToken.None));

        google.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new GoogleIdTokenPayload("sub", "a@b.com", true, null, null));
        identity.FindOrCreateGoogleUserAsync(Arg.Any<GoogleIdTokenPayload>(), Arg.Any<CancellationToken>()).Returns(User(active: false));
        await Assert.ThrowsAsync<AccountDisabledException>(() => handler.Handle(new GoogleLoginCommand("a.b.c"), CancellationToken.None));
    }

    [Fact]
    public async Task Google_login_uses_common_token_issuance()
    {
        var google = Substitute.For<IGoogleIdTokenValidator>();
        var identity = Substitute.For<IIdentityService>();
        var factory = Factory(identity, out var tokens, out var users, out var uow);
        var user = User();
        google.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new GoogleIdTokenPayload("sub", "a@b.com", true, null, null));
        identity.FindOrCreateGoogleUserAsync(Arg.Any<GoogleIdTokenPayload>(), Arg.Any<CancellationToken>()).Returns(user);
        ArrangeIssue(identity, tokens, users, uow, user);
        var response = await new GoogleLoginCommandHandler(google, identity, factory).Handle(new GoogleLoginCommand("a.b.c"), CancellationToken.None);
        Assert.Equal("jwt", response.AccessToken);
    }

    [Fact]
    public async Task Logout_is_idempotent_and_revokes_matching_owned_token()
    {
        var current = Current("user-1"); var tokens = Substitute.For<ITokenService>(); var users = Substitute.For<IUserRepository>(); var uow = Substitute.For<IUnitOfWork>();
        uow.Users.Returns(users); tokens.HashToken("raw").Returns("hash");
        users.ExecuteForUserAsync<bool>(Arg.Any<string>(), Arg.Any<Func<CancellationToken, Task<bool>>>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<Func<CancellationToken, Task<bool>>>(1)(CancellationToken.None));
        users.GetRefreshTokenByHashAsync("hash", Arg.Any<CancellationToken>()).Returns(Token("user-1", "hash"));
        var handler = new LogoutCommandHandler(current, tokens, uow, Clock);
        await handler.Handle(new LogoutCommand("raw"), CancellationToken.None);
        await users.Received(1).RevokeRefreshTokenIfActiveAsync(Arg.Any<Guid>(), Now.UtcDateTime, null, Arg.Any<CancellationToken>());
        await handler.Handle(new LogoutCommand(""), CancellationToken.None);
    }

    [Fact]
    public async Task Password_commands_change_schedule_and_map_reset_results()
    {
        var current = Current("user-1", Guid.NewGuid()); var identity = Substitute.For<IIdentityService>();
        await new ChangePasswordCommandHandler(current, identity, NullLogger<ChangePasswordCommandHandler>.Instance).Handle(new ChangePasswordCommand("OldPassword1!", "NewPassword1!"), CancellationToken.None);
        await identity.Received(1).ChangePasswordAsync("user-1", "OldPassword1!", "NewPassword1!", current.SessionId, Arg.Any<CancellationToken>());

        var scheduler = Substitute.For<IPasswordResetEmailScheduler>();
        identity.FindByEmailAsync("a@b.com", Arg.Any<CancellationToken>()).Returns(User());
        await new ForgotPasswordCommandHandler(identity, scheduler).Handle(new ForgotPasswordCommand(" a@b.com ", "correlation"), CancellationToken.None);
        scheduler.Received(1).SchedulePasswordResetEmail("user-1", "correlation");

        identity.ResetPasswordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(PasswordResetStatus.Disabled);
        await Assert.ThrowsAsync<AccountDisabledException>(() => new ResetPasswordCommandHandler(identity).Handle(new ResetPasswordCommand("a@b.com", "token", "NewPassword1!"), CancellationToken.None));
        identity.ResetPasswordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(PasswordResetStatus.InvalidToken);
        await Assert.ThrowsAsync<PasswordResetTokenInvalidException>(() => new ResetPasswordCommandHandler(identity).Handle(new ResetPasswordCommand("a@b.com", "token", "NewPassword1!"), CancellationToken.None));
    }

    [Fact]
    public async Task Forgot_password_does_not_schedule_for_unknown_or_disabled_account()
    {
        var identity = Substitute.For<IIdentityService>(); var scheduler = Substitute.For<IPasswordResetEmailScheduler>();
        identity.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((IdentityUserInfo?)null);
        await new ForgotPasswordCommandHandler(identity, scheduler).Handle(new ForgotPasswordCommand("unknown@example.com", "c"), CancellationToken.None);
        identity.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(User(active: false));
        await new ForgotPasswordCommandHandler(identity, scheduler).Handle(new ForgotPasswordCommand("disabled@example.com", "c"), CancellationToken.None);
        scheduler.DidNotReceiveWithAnyArgs().SchedulePasswordResetEmail(default!, default!);
    }

    [Fact]
    public async Task Profile_queries_map_and_preserve_field_presence()
    {
        var current = Current("user-1"); var identity = Substitute.For<IIdentityService>();
        identity.FindByIdAsync("user-1", Arg.Any<CancellationToken>()).Returns(User());
        var profile = await new GetCurrentUserQueryHandler(current, identity).Handle(new GetCurrentUserQuery(), CancellationToken.None);
        Assert.Equal("Jane", profile.DisplayName);

        identity.UpdateProfileAsync("user-1", Arg.Any<ProfileChanges>(), Arg.Any<CancellationToken>()).Returns(User(avatar: "https://cdn/avatar.png"));
        await new UpdateProfileCommandHandler(current, identity).Handle(new UpdateProfileCommand(" Jane ", " https://cdn/avatar.png ", " bio "), CancellationToken.None);
        await identity.Received(1).UpdateProfileAsync("user-1", Arg.Is<ProfileChanges>(x => x.DisplayName == "Jane" && x.AvatarUrl == "https://cdn/avatar.png" && x.Bio == "bio" && x.HasBio), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Profile_handlers_require_current_existing_user()
    {
        var identity = Substitute.For<IIdentityService>();
        await Assert.ThrowsAsync<InvalidTokenException>(() => new GetCurrentUserQueryHandler(Current(null), identity).Handle(new GetCurrentUserQuery(), CancellationToken.None));
        identity.FindByIdAsync("user-1", Arg.Any<CancellationToken>()).Returns((IdentityUserInfo?)null);
        await Assert.ThrowsAsync<InvalidTokenException>(() => new GetCurrentUserQueryHandler(Current("user-1"), identity).Handle(new GetCurrentUserQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Sessions_list_revoke_one_and_revoke_all_are_scoped_to_current_user()
    {
        var users = Substitute.For<IUserRepository>(); var uow = Substitute.For<IUnitOfWork>(); uow.Users.Returns(users);
        var active = Token("user-1", "a");
        var current = Current("user-1", active.Id);
        users.GetActiveRefreshTokensAsync("user-1", Now.UtcDateTime, Arg.Any<CancellationToken>()).Returns(new[] { active });
        var sessions = await new GetMySessionsQueryHandler(current, uow, Clock).Handle(new GetMySessionsQuery(), CancellationToken.None);
        Assert.True(sessions.Single().IsCurrent);
        users.ExecuteForUserAsync<bool>(Arg.Any<string>(), Arg.Any<Func<CancellationToken, Task<bool>>>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<Func<CancellationToken, Task<bool>>>(1)(CancellationToken.None));
        users.GetRefreshTokenByIdAsync("user-1", active.Id, Arg.Any<CancellationToken>()).Returns(active);
        Assert.True(await new RevokeSessionCommandHandler(current, uow, Clock).Handle(new RevokeSessionCommand(active.Id), CancellationToken.None));
        users.ExecuteForUserAsync<bool>(Arg.Any<string>(), Arg.Any<Func<CancellationToken, Task<bool>>>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<Func<CancellationToken, Task<bool>>>(1)(CancellationToken.None));
        await new RevokeAllSessionsCommandHandler(current, uow, Clock).Handle(new RevokeAllSessionsCommand(), CancellationToken.None);
        await users.Received().RevokeAllRefreshTokensAsync("user-1", Now.UtcDateTime, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task User_administration_queries_and_status_changes_are_safe()
    {
        var users = Substitute.For<IUserRepository>(); var uow = Substitute.For<IUnitOfWork>(); uow.Users.Returns(users);
        var page = new PagedResult<UserAdminDto>([], 0, 1, 12); users.GetUsersPageAsync(Arg.Any<UserListCriteria>(), Arg.Any<CancellationToken>()).Returns(page);
        Assert.Same(page, await new GetUsersQueryHandler(uow).Handle(new GetUsersQuery(Search: " jane "), CancellationToken.None));
        await users.Received().GetUsersPageAsync(Arg.Is<UserListCriteria>(x => x.Search == "jane"), Arg.Any<CancellationToken>());

        var current = Current("admin"); var identity = Substitute.For<IIdentityService>();
        var handler = new SetUserStatusCommandHandler(identity, current, NullLogger<SetUserStatusCommandHandler>.Instance);
        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new SetUserStatusCommand("admin", false, "valid reason"), CancellationToken.None));
        identity.SetActiveAsync("target", false, Arg.Any<CancellationToken>()).Returns(User(id: "target", active: false));
        var result = await handler.Handle(new SetUserStatusCommand("target", false, "valid reason"), CancellationToken.None);
        Assert.False(result.IsActive);
        identity.SetActiveAsync("missing", true, Arg.Any<CancellationToken>()).Returns((IdentityUserInfo?)null);
        await Assert.ThrowsAsync<UserNotFoundException>(() => handler.Handle(new SetUserStatusCommand("missing", true, "valid reason"), CancellationToken.None));
    }

    [Fact]
    public async Task Refresh_rejects_unknown_expired_disabled_and_reused_tokens()
    {
        var identity = Substitute.For<IIdentityService>(); var tokens = Substitute.For<ITokenService>(); var users = Substitute.For<IUserRepository>(); var uow = Substitute.For<IUnitOfWork>(); uow.Users.Returns(users); tokens.HashToken("raw").Returns("hash");
        var handler = new RefreshTokenCommandHandler(tokens, identity, uow, Clock, NullLogger<RefreshTokenCommandHandler>.Instance);
        users.GetRefreshTokenByHashAsync("hash", Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);
        await Assert.ThrowsAsync<InvalidTokenException>(() => handler.Handle(new RefreshTokenCommand("raw"), CancellationToken.None));

        users.GetRefreshTokenByHashAsync("hash", Arg.Any<CancellationToken>()).Returns(Token("user-1", "hash", Now.AddMinutes(-1).UtcDateTime));
        var expired = await Assert.ThrowsAsync<InvalidTokenException>(() => handler.Handle(new RefreshTokenCommand("raw"), CancellationToken.None));
        Assert.Equal(ErrorCodes.AuthRefreshTokenExpired, expired.Code);

        var revoked = Token("user-1", "hash"); revoked.Revoke(Now.UtcDateTime, "next");
        users.GetRefreshTokenByHashAsync("hash", Arg.Any<CancellationToken>()).Returns(revoked);
        users.ExecuteForUserAsync<AuthResponseDto?>(Arg.Any<string>(), Arg.Any<Func<CancellationToken, Task<AuthResponseDto?>>>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<Func<CancellationToken, Task<AuthResponseDto?>>>(1)(CancellationToken.None));
        identity.FindByIdAsync("user-1", Arg.Any<CancellationToken>()).Returns(User()); identity.IsLockedOutAsync("user-1", Arg.Any<CancellationToken>()).Returns(false);
        var reuse = await Assert.ThrowsAsync<InvalidTokenException>(() => handler.Handle(new RefreshTokenCommand("raw"), CancellationToken.None));
        Assert.Equal(ErrorCodes.AuthRefreshTokenRevoked, reuse.Code);
        await users.Received().RevokeRefreshTokenFamilyAsync("user-1", "hash", Now.UtcDateTime, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Refresh_rotates_token_and_preserves_session_claim()
    {
        var identity = Substitute.For<IIdentityService>(); var tokens = Substitute.For<ITokenService>(); var users = Substitute.For<IUserRepository>(); var uow = Substitute.For<IUnitOfWork>(); uow.Users.Returns(users); tokens.HashToken("raw").Returns("hash");
        var token = Token("user-1", "hash"); users.GetRefreshTokenByHashAsync("hash", Arg.Any<CancellationToken>()).Returns(token);
        users.ExecuteForUserAsync<AuthResponseDto?>(Arg.Any<string>(), Arg.Any<Func<CancellationToken, Task<AuthResponseDto?>>>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<Func<CancellationToken, Task<AuthResponseDto?>>>(1)(CancellationToken.None));
        identity.FindByIdAsync("user-1", Arg.Any<CancellationToken>()).Returns(User()); identity.IsLockedOutAsync("user-1", Arg.Any<CancellationToken>()).Returns(false);
        tokens.CreateRefreshToken().Returns(new GeneratedRefreshToken("replacement", "replacement-hash", Now.AddDays(7).UtcDateTime)); tokens.CreateAccessToken(Arg.Any<IdentityUserInfo>(), Arg.Any<Guid?>()).Returns(new AccessToken("jwt", Now.AddMinutes(15).UtcDateTime, 900));
        users.RevokeRefreshTokenIfActiveAsync(token.Id, Now.UtcDateTime, "replacement-hash", Arg.Any<CancellationToken>()).Returns(true); uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        var response = await new RefreshTokenCommandHandler(tokens, identity, uow, Clock, NullLogger<RefreshTokenCommandHandler>.Instance).Handle(new RefreshTokenCommand("raw") { IpAddress = "10.0.0.1" }, CancellationToken.None);
        Assert.Equal("replacement", response.RefreshToken); await users.Received().AddRefreshTokenAsync(Arg.Is<RefreshToken>(x => x.TokenHash == "replacement-hash"), Arg.Any<CancellationToken>());
    }

    private static AuthResponseFactory Factory(IIdentityService identity, out ITokenService tokens, out IUserRepository users, out IUnitOfWork uow)
    {
        tokens = Substitute.For<ITokenService>(); users = Substitute.For<IUserRepository>(); uow = Substitute.For<IUnitOfWork>(); uow.Users.Returns(users);
        return new AuthResponseFactory(tokens, identity, uow, Clock);
    }

    private static void ArrangeIssue(IIdentityService identity, ITokenService tokens, IUserRepository users, IUnitOfWork uow, IdentityUserInfo user)
    {
        identity.FindByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        tokens.CreateRefreshToken().Returns(new GeneratedRefreshToken("raw-refresh", "hash-refresh", Now.AddDays(7).UtcDateTime));
        tokens.CreateAccessToken(Arg.Any<IdentityUserInfo>(), Arg.Any<Guid?>()).Returns(new AccessToken("jwt", Now.AddMinutes(15).UtcDateTime, 900));
        users.ExecuteForUserAsync<AuthResponseDto>(Arg.Any<string>(), Arg.Any<Func<CancellationToken, Task<AuthResponseDto>>>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<Func<CancellationToken, Task<AuthResponseDto>>>(1)(CancellationToken.None));
        uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    private static ICurrentUser Current(string? id, Guid? sessionId = null)
    {
        var current = Substitute.For<ICurrentUser>(); current.UserId.Returns(id); current.SessionId.Returns(sessionId); current.Principal.Returns(new ClaimsPrincipal()); return current;
    }

    private static IdentityUserInfo User(string id = "user-1", bool active = true, string? avatar = null) => new(id, "Jane", "jane@example.com", "jane", avatar, active, [Roles.Author], "bio", Now.UtcDateTime);
    private static RefreshToken Token(string userId, string hash, DateTime? expires = null) => RefreshToken.Create(userId, hash, expires ?? Now.AddDays(7).UtcDateTime, Now.UtcDateTime, "127.0.0.1");

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

#pragma warning restore SA1516
#pragma warning restore SA1122
#pragma warning restore SA1107

using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Infrastructure.Jobs;

namespace CulinaryBlog.API.IntegrationTests.Auth;

/// <summary>
/// Trả nợ test Buổi 2 (BAO_CAO_BUOI_2.md §6): FR-AUTH-001 đăng ký và FR-AUTH-002 đăng nhập,
/// mỗi endpoint ≥ 1 happy path + các nhánh lỗi (NFR-MAINT-002).
///
/// Đây là lớp test DUY NHẤT tạo tài khoản mới (và làm khóa tài khoản), nên nó dùng Respawn để tự dọn
/// trước mỗi test: nếu không, bảng AspNetUsers cứ phình ra theo từng lần chạy và một tài khoản bị khóa
/// ở test này sẽ làm test khác thất bại theo thứ tự chạy — đúng loại test "thỉnh thoảng đỏ" khó truy nhất.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class AuthEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    private static readonly string[] EmailField = ["email"];

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_WithValidPayload_Returns201WithTokensAndEnqueuesWelcomeEmail()
    {
        var client = factory.CreateClient();
        var suffix = NewSuffix();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", NewRegistration(suffix));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var auth = await response.ReadAsAsync<AuthResponseDto>();
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));
        Assert.Equal($"it-{suffix}@culinaryblog.test", auth.User.Email);

        // FR-AUTH-001 bước 11: tài khoản mới luôn được xếp hàng gửi email chào mừng (FR-JOB-001).
        Assert.True(
            factory.BackgroundJobs.WasEnqueued<WelcomeEmailJob>(nameof(WelcomeEmailJob.ExecuteAsync), auth.User.Id),
            "Đăng ký thành công nhưng không enqueue WelcomeEmailJob.");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var client = factory.CreateClient();
        var payload = NewRegistration(NewSuffix());

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/v1/auth/register", payload)).StatusCode);

        var second = await client.PostAsJsonAsync("/api/v1/auth/register", payload);

        await second.ShouldBeProblemAsync(HttpStatusCode.Conflict, ErrorCodes.AuthEmailExists);
    }

    /// <summary>D-11 / MT-08: lỗi validation trả 400, KHÔNG phải 422.</summary>
    [Fact]
    public async Task Register_WithWeakPasswordAndBadEmail_Returns400WithFieldErrors()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            fullName = "A",
            email = "khong-phai-email",
            userName = "it_invalid",
            password = "123",
        });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);

        var errors = await response.ReadValidationErrorsAsync();
        Assert.Contains("email", errors.Keys, StringComparer.Ordinal);
        Assert.Contains("password", errors.Keys, StringComparer.Ordinal);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_Returns200()
    {
        var client = factory.CreateClient();
        var suffix = NewSuffix();
        await client.PostAsJsonAsync("/api/v1/auth/register", NewRegistration(suffix));

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = $"it-{suffix}@culinaryblog.test",
            password = TestDataSeeder.ValidPassword,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.ReadAsAsync<AuthResponseDto>();
        Assert.Contains("Author", auth.User.Roles, StringComparer.Ordinal);
    }

    /// <summary>FR-AUTH-002 A1: thông báo chung, không tiết lộ email có tồn tại hay không (chống user enumeration).</summary>
    [Fact]
    public async Task Login_WithWrongPassword_Returns401GenericMessage()
    {
        var client = factory.CreateClient();
        var suffix = NewSuffix();
        await client.PostAsJsonAsync("/api/v1/auth/register", NewRegistration(suffix));

        var wrongPassword = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = $"it-{suffix}@culinaryblog.test",
            password = "SaiMatKhau#1",
        });
        var unknownEmail = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = $"khong-ton-tai-{NewSuffix()}@culinaryblog.test",
            password = "SaiMatKhau#1",
        });

        var first = await wrongPassword.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, ErrorCodes.AuthInvalidCredentials);
        var second = await unknownEmail.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, ErrorCodes.AuthInvalidCredentials);

        Assert.Equal(first.Detail, second.Detail);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_Returns400()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = "a@b.com", password = string.Empty });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    /// <summary>FR-AUTH-002 A3: sai 5 lần → khóa 15 phút (423 Locked). Lần thứ 6 vẫn 423 dù mật khẩu ĐÚNG.</summary>
    [Fact]
    public async Task Login_AfterFiveFailedAttempts_LocksAccountWith423()
    {
        var client = factory.CreateClient();
        var suffix = NewSuffix();
        var email = $"it-{suffix}@culinaryblog.test";
        await client.PostAsJsonAsync("/api/v1/auth/register", NewRegistration(suffix));

        HttpResponseMessage? last = null;
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            last = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "SaiMatKhau#1" });
        }

        // Lần sai thứ 5 là lần khiến Identity đặt LockoutEnd.
        var locked = await last!.ShouldBeProblemAsync(HttpStatusCode.Locked, ErrorCodes.AuthAccountLocked);
        Assert.True(locked.Extensions.ContainsKey("retryAfterMinutes"), "Thiếu retryAfterMinutes trong Problem Details.");

        // Mật khẩu đúng cũng không vào được khi đang bị khóa.
        var correctButLocked = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = TestDataSeeder.ValidPassword });
        await correctButLocked.ShouldBeProblemAsync(HttpStatusCode.Locked, ErrorCodes.AuthAccountLocked);
    }

    [Fact]
    public async Task Register_WithInvalidEmailFormat_ReportsErrorOnEmailFieldOnly()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            fullName = "Nguyễn Văn Test",
            email = "thieu-cha-a-cong",
            userName = $"it_{NewSuffix()}",
            password = TestDataSeeder.ValidPassword,
        });

        var errors = await response.ReadValidationErrorsAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(EmailField, errors.Keys.Order(StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public async Task Register_WithTooLongFullName_Returns400()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            fullName = new string('a', 101),
            email = $"it-{NewSuffix()}@culinaryblog.test",
            userName = $"it_{NewSuffix()}",
            password = TestDataSeeder.ValidPassword,
        });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
        Assert.Contains("fullName", (await response.ReadValidationErrorsAsync()).Keys, StringComparer.Ordinal);
    }

    private static object NewRegistration(string suffix) => new
    {
        fullName = "Nguyễn Văn Test",
        email = $"it-{suffix}@culinaryblog.test",
        userName = $"it_{suffix}",
        password = TestDataSeeder.ValidPassword,
    };

    private static string NewSuffix() => Guid.NewGuid().ToString("N")[..12];
}

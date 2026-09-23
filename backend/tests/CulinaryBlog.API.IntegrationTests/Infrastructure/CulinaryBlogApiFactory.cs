using System.Data.Common;
using System.Net.Http.Headers;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using CulinaryBlog.API.IntegrationTests.Infrastructure.Fakes;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Infrastructure.Persistence;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Respawn;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace CulinaryBlog.API.IntegrationTests.Infrastructure;

/// <summary>
/// NFR-MAINT-002 – hạ tầng integration test dùng chung cho cả nhóm (Buổi 3, Dev 4).
///
/// Dựng ba dịch vụ thật bằng Testcontainers: <c>postgres:16-alpine</c>, <c>redis:7-alpine</c> và MinIO —
/// đúng bộ phụ thuộc runtime trong docker-compose.yml (SRS §6.5). Không dùng EF Core In-Memory vì provider
/// đó không có UNIQUE, FK cascade, partial index, tsvector hay concurrency token thật — tức là đúng những
/// thứ cần kiểm chứng nhất (MT-03, MT-05, MT-09, MT-25) đều biến mất; test xanh trên In-Memory rồi vỡ trên
/// PostgreSQL thật là kịch bản tệ nhất vì nó tạo ra niềm tin sai.
///
/// ⚠️ Testcontainers KHÔNG chạy <c>docker/postgres/init.sql</c>, nên mọi extension/DDL mà code cần phải nằm
/// trong migration (điều kiện tiên quyết của D-18, Buổi 4). Migration B1 đã khai báo unaccent + pg_trgm.
/// </summary>
public sealed class CulinaryBlogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>Đủ dài cho HS256 – chỉ dùng trong test, không phải secret thật (NFR-SEC-007).</summary>
    private const string TestSigningKey = "integration-tests-only-signing-key-0123456789-0123456789-0123456789";

    private const string TestBucketName = "culinary-blog-tests";

    /// <summary>
    /// Ghim tag thay vì "latest" để kết quả test không đổi theo ngày chạy, và lấy từ **quay.io** —
    /// repository "minio/minio" trên Docker Hub đã bị gỡ (mọi lần pull trả "pull access denied ...
    /// repository does not exist"), quay.io là registry chính thức MinIO còn phát hành.
    /// </summary>
    private const string MinioImage = "quay.io/minio/minio:RELEASE.2025-09-07T16-13-09Z";

    private const string MinioAccessKey = "minioadmin";
    private const string MinioSecretKey = "minioadmin";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("culinaryblog_tests")
        .WithUsername("culinary")
        .WithPassword("culinary_tests")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .Build();

    private readonly MinioContainer _minio = new MinioBuilder(MinioImage)
        .WithUsername(MinioAccessKey)
        .WithPassword(MinioSecretKey)
        .Build();

    private DbConnection? _respawnConnection;
    private Respawner? _respawner;

    /// <summary>Ghi lại mọi lời gọi Hangfire mà không cần BackgroundJobServer chạy.</summary>
    public FakeBackgroundJobClient BackgroundJobs { get; } = new();

    /// <summary>Endpoint S3 của container MinIO (dùng cho cả cấu hình app lẫn kiểm chứng trực tiếp trong test).</summary>
    public string MinioEndpoint => $"http://{_minio.Hostname}:{_minio.GetMappedPublicPort(9000)}";

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync(), _minio.StartAsync()).ConfigureAwait(false);

        // Truy cập Services buộc host khởi tạo (migration/seed ở startup đã tắt để test tự kiểm soát thời điểm).
        using (var scope = Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().Database.MigrateAsync().ConfigureAwait(false);
            await EnsureBucketAsync(scope.ServiceProvider).ConfigureAwait(false);
            await TestDataSeeder.SeedAsync(scope.ServiceProvider).ConfigureAwait(false);
        }

        // Respawn dọn dữ liệu test sinh ra. Giữ __EFMigrationsHistory (xóa đi thì lần Migrate sau chạy lại
        // toàn bộ DDL trên schema đã tồn tại) và chỉ đụng schema "public" (schema "hangfire" do Hangfire quản).
        _respawnConnection = new NpgsqlConnection(_postgres.GetConnectionString());
        await _respawnConnection.OpenAsync().ConfigureAwait(false);
        _respawner = await Respawner.CreateAsync(_respawnConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"],
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Xóa mọi dữ liệu test sinh ra rồi nạp lại bộ dữ liệu cố định (<see cref="TestDataSeeder"/>).
    /// Lớp test nào có ghi dữ liệu thì gọi ở <c>InitializeAsync</c> của mình để không ảnh hưởng lớp khác.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null || _respawnConnection is null)
        {
            throw new InvalidOperationException("InitializeAsync của factory chưa chạy xong.");
        }

        await _respawner.ResetAsync(_respawnConnection).ConfigureAwait(false);
        BackgroundJobs.Clear();

        using var scope = Services.CreateScope();
        await TestDataSeeder.SeedAsync(scope.ServiceProvider).ConfigureAwait(false);
    }

    /// <summary>
    /// HttpClient mang sẵn JWT THẬT (ký bằng <see cref="ITokenService"/> của ứng dụng, không phải handler
    /// xác thực giả) — nhờ vậy mọi dev viết được test cho Guest, Author và Admin mà không phải đăng nhập thật.
    /// </summary>
    /// <param name="roles">Một hoặc nhiều role, phân tách bằng dấu phẩy: "Author", "Admin", "Author,Admin".</param>
    /// <param name="userId">
    /// Id (GUID dạng chuỗi – khớp kiểu Id của ASP.NET Core Identity) của user mà token đại diện.
    /// Bỏ trống ⇒ dùng tài khoản Author cố định của bộ dữ liệu test.
    /// </param>
    public HttpClient CreateClientAs(string roles, string? userId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roles);

        var roleList = roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var id = userId ?? TestDataSeeder.AuthorUserId;

        var user = new IdentityUserInfo(
            id,
            $"Integration Test {string.Join('+', roleList)}",
            $"{id}@integration.test",
            $"it_{id.Replace("-", string.Empty, StringComparison.Ordinal)}",
            AvatarUrl: null,
            IsActive: true,
            roleList);

        var token = Services.GetRequiredService<ITokenService>().CreateAccessToken(user);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }

    /// <inheritdoc cref="CreateClientAs(string, string?)"/>
    public HttpClient CreateClientAs(string roles, Guid userId) =>
        CreateClientAs(roles, userId.ToString());

    public override async ValueTask DisposeAsync()
    {
        if (_respawnConnection is not null)
        {
            await _respawnConnection.DisposeAsync().ConfigureAwait(false);
        }

        await base.DisposeAsync().ConfigureAwait(false);

        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _redis.DisposeAsync().AsTask(),
            _minio.DisposeAsync().AsTask()).ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }

    /// <summary>xUnit v2 gọi bản Task này khi collection fixture kết thúc; dọn dẹp nằm ở bản ValueTask.</summary>
    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync().ConfigureAwait(false);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment(Environments.Development);

        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:Redis", $"{_redis.Hostname}:{_redis.GetMappedPublicPort(6379)}");

        builder.UseSetting("MinIO:Endpoint", MinioEndpoint);
        builder.UseSetting("MinIO:PublicBaseUrl", $"{MinioEndpoint}/{TestBucketName}");
        builder.UseSetting("MinIO:AccessKey", MinioAccessKey);
        builder.UseSetting("MinIO:SecretKey", MinioSecretKey);
        builder.UseSetting("MinIO:BucketName", TestBucketName);

        builder.UseSetting("Jwt:SigningKey", TestSigningKey);

        // Migration + seed do InitializeAsync chủ động gọi; tắt ở startup để test kiểm soát được thời điểm.
        builder.UseSetting("Database:MigrateOnStartup", "false");
        builder.UseSetting("Seed:Enabled", "false");

        // Không chạy BackgroundJobServer: job chạy song song làm kết quả test mất tính tất định.
        builder.UseSetting("Hangfire:ServerEnabled", "false");
        builder.UseSetting("Hangfire:WorkerOnly", "false");
        builder.UseSetting("Hangfire:DashboardGateSecret", TestDataSeeder.HangfireGateSecret);

        builder.ConfigureServices(services =>
        {
            // IBackgroundJobClient thật ghi job vào storage rồi chờ worker; bản ghi lại đủ để khẳng định
            // "endpoint CÓ enqueue đúng job" — bản thân WelcomeEmailJob đã chạy thật từ Buổi 2.
            services.RemoveAll<IBackgroundJobClient>();
            services.AddSingleton<IBackgroundJobClient>(BackgroundJobs);
        });
    }

    /// <summary>
    /// <c>MinioBucketInitializer</c> là BackgroundService nên chạy bất đồng bộ với test đầu tiên.
    /// Tạo bucket ngay tại đây để test upload không phụ thuộc vào thời điểm hosted service hoàn tất.
    /// </summary>
    private static async Task EnsureBucketAsync(IServiceProvider services)
    {
        var s3 = services.GetRequiredService<IAmazonS3>();

        if (!await AmazonS3Util.DoesS3BucketExistV2Async(s3, TestBucketName).ConfigureAwait(false))
        {
            await s3.PutBucketAsync(new PutBucketRequest { BucketName = TestBucketName }).ConfigureAwait(false);
        }
    }
}

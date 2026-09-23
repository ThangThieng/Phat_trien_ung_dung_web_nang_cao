using Amazon.Runtime;
using Amazon.S3;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Infrastructure.Caching;
using CulinaryBlog.Infrastructure.Email;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Jobs;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Interceptors;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using CulinaryBlog.Infrastructure.Persistence.Seed;
using CulinaryBlog.Infrastructure.Storage;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection.");

        AddPersistence(services, connectionString);
        AddIdentity(services, configuration);
        AddCaching(services, configuration);
        AddStorage(services, configuration);
        AddJobs(services, configuration, connectionString);

        services.AddOptions<SeedOptions>().Bind(configuration.GetSection(SeedOptions.SectionName));
        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, string connectionString)
    {
        services.AddSingleton<AuditInterceptor>();

        // EnableRetryOnFailure: Postgres khởi động chậm hơn API (57P03) hoặc mất kết nối tạm thời → retry thay vì crash.
        // Ignore 20606: RecipeNutrition là optional dependent có chủ đích (§7.2.1) – mọi cột Nutrition_* null ⇒ Nutrition = null.
        services.AddDbContext<CulinaryBlogDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql
                    .MigrationsHistoryTable("__EFMigrationsHistory")
                    .EnableRetryOnFailure(maxRetryCount: 6, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null))
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.OptionalDependentWithoutIdentifyingPropertyWarning))
                .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CulinaryBlogDbContext>());
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRecipeReadRepository, RecipeReadRepository>();
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
    }

    private static void AddIdentity(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // NFR-SEC-001: ≥ 8 ký tự, hoa + thường + số + ký tự đặc biệt
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;

                // FR-AUTH-002 A3: sai 5 lần → khóa 15 phút
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<CulinaryBlogDbContext>();

        // NFR-SEC-001: IPasswordHasher<ApplicationUser> = PBKDF2-HMAC-SHA512 (Identity V3), 100.000 vòng lặp
        services.Configure<PasswordHasherOptions>(o =>
        {
            o.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            o.IterationCount = 100_000;
        });

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var redis = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Thiếu ConnectionStrings:Redis.");

        services.AddStackExchangeRedisCache(o =>
        {
            // abortConnect=false: Redis down không làm crash app; timeout ngắn để fallback DB nhanh (NFR-REL-002)
            o.Configuration = $"{redis},abortConnect=false,connectTimeout=2000,syncTimeout=2000";
            o.InstanceName = "culinaryblog:";
        });
        services.AddSingleton<ICacheService, RedisCacheService>();
    }

    private static void AddStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MinioOptions>()
            .Bind(configuration.GetSection(MinioOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var minio = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
            var config = new AmazonS3Config
            {
                ServiceURL = minio.Endpoint,
                ForcePathStyle = true,
                AuthenticationRegion = "us-east-1",
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            };
            return new AmazonS3Client(new BasicAWSCredentials(minio.AccessKey, minio.SecretKey), config);
        });
        services.AddSingleton<IFileStorageService, MinioFileStorageService>();
        services.AddHostedService<MinioBucketInitializer>();
    }

    private static void AddJobs(IServiceCollection services, IConfiguration configuration, string connectionString)
    {
        services.AddOptions<SmtpOptions>().Bind(configuration.GetSection(SmtpOptions.SectionName));
        services.AddTransient<IEmailService, MailKitEmailService>();
        services.AddScoped<WelcomeEmailJob>();
        services.AddScoped<IWelcomeEmailScheduler, HangfireWelcomeEmailScheduler>();

        // Tồn đọng Buổi 2 §6: mặc định 15 giây khiến job fire-and-forget (Welcome Email) mãi mới chạy,
        // quá chậm khi demo và khi viết integration test. Môi trường Development hạ xuống 1 giây, đổi lại
        // là vài truy vấn polling mỗi giây trên Postgres, chấp nhận được ở môi trường dev.
        var pollInterval = TimeSpan.FromSeconds(configuration.GetValue("Hangfire:PollIntervalSeconds", 15));

        // SRS §3.6: Hangfire dùng chung PostgreSQL (schema "hangfire")
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                o => o.UseNpgsqlConnection(connectionString),
                new PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire",
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = pollInterval,
                }));

        // Server xử lý job: bật ở container "hangfire" (worker) hoặc khi chạy local (Hangfire:ServerEnabled mặc định true)
        if (configuration.GetValue("Hangfire:ServerEnabled", true) || configuration.GetValue<bool>("Hangfire:WorkerOnly"))
        {
            // SchedulePollingInterval chi phối job ĐÃ LÊN LỊCH (các lần retry 1'/5'/30' của WelcomeEmailJob).
            services.AddHangfireServer(o =>
            {
                o.ServerName = $"culinaryblog-{Environment.MachineName}";
                o.SchedulePollingInterval = pollInterval;
            });
        }
    }
}

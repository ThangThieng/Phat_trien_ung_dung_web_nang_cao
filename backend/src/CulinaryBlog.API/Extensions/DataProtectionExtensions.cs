using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;

namespace CulinaryBlog.API.Extensions;

/// <summary>Cấu hình <c>DataProtection:*</c> (biến môi trường <c>DataProtection__KeysPath</c>, <c>DataProtection__CertificatePath</c>…).</summary>
public sealed class DataProtectionSettings
{
    public const string SectionName = "DataProtection";

    /// <summary>Thư mục lưu khóa — volume dùng chung giữa <c>api</c> và <c>hangfire</c> (image đặt sẵn <c>/home/app/dpkeys</c>).</summary>
    public string? KeysPath { get; set; }

    /// <summary>File PFX (khóa riêng RSA) dùng mã hóa khóa trên đĩa — bắt buộc ở Production.</summary>
    public string? CertificatePath { get; set; }

    public string? CertificatePassword { get; set; }
}

/// <summary>
/// DataProtection dùng chung cho container <c>api</c> và <c>hangfire</c> (NFR-SCALE-001): cùng tên ứng dụng, cùng thư mục khóa trên
/// volume, nên dữ liệu một container bảo vệ thì container kia đọc được — ví dụ token đặt lại mật khẩu do worker sinh (FR-AUTH-011)
/// được API xác minh.
/// <para>
/// <b>Retrofit D-17 (Buổi 7):</b> từ Buổi 2 khóa nằm <b>dạng rõ</b> trên volume (cảnh báo <c>XmlKeyManager[35]</c>). Ai đọc được
/// volume là giả mạo được token đặt lại mật khẩu và mọi dữ liệu do DataProtection ký. Khi có <c>CertificatePath</c>, khóa mới được
/// mã hóa bằng khóa công khai của chứng chỉ (<c>ProtectKeysWithCertificate</c>) và chỉ giải mã được khi có khóa riêng
/// (<c>UnprotectKeysWithAnyCertificate</c> — nạp thẳng từ file, không cần kho chứng chỉ của hệ điều hành, vốn không có trong
/// container Linux). Production thiếu chứng chỉ thì <b>không khởi động</b> thay vì âm thầm ghi khóa dạng rõ — cùng cách kiểm cấu
/// hình lúc khởi động của CORS (Buổi 5).
/// </para>
/// </summary>
public static class DataProtectionExtensions
{
    public const string ApplicationName = "CulinaryBlog";

    public static IDataProtectionBuilder AddCulinaryDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var settings = configuration.GetSection(DataProtectionSettings.SectionName).Get<DataProtectionSettings>()
            ?? new DataProtectionSettings();
        var builder = services.AddDataProtection().SetApplicationName(ApplicationName);

        // Không có thư mục khóa (chạy "dotnet run", integration test): khóa theo mặc định của ASP.NET Core, không lưu lên volume.
        if (string.IsNullOrWhiteSpace(settings.KeysPath))
        {
            return builder;
        }

        builder.PersistKeysToFileSystem(new DirectoryInfo(settings.KeysPath));

        if (!string.IsNullOrWhiteSpace(settings.CertificatePath))
        {
            var certificate = LoadCertificate(settings.CertificatePath, settings.CertificatePassword);
            builder.ProtectKeysWithCertificate(certificate).UnprotectKeysWithAnyCertificate(certificate);
        }
        else if (environment.IsProduction())
        {
            throw new InvalidOperationException(
                "D-17 / NFR-SEC-007: Production phải mã hóa khóa DataProtection trên volume. Đặt DataProtection__CertificatePath "
                + "(file PFX có khóa riêng RSA) và DataProtection__CertificatePassword — xem docker-compose.prod.yml.");
        }

        return builder;
    }

    /// <summary>Nạp PFX từ file; chứng chỉ phải có khóa riêng RSA (EncryptedXml dùng RSA để bọc khóa).</summary>
    public static X509Certificate2 LoadCertificate(string path, string? password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // EphemeralKeySet: khóa riêng chỉ nằm trong bộ nhớ của process, không ghi ra kho khóa của hệ điều hành.
        var certificate = X509CertificateLoader.LoadPkcs12FromFile(path, password, X509KeyStorageFlags.EphemeralKeySet);
        using var publicKey = certificate.GetRSAPublicKey();
        if (!certificate.HasPrivateKey || publicKey is null)
        {
            certificate.Dispose();
            throw new InvalidOperationException(
                $"Chứng chỉ DataProtection '{path}' phải là chứng chỉ RSA kèm khóa riêng (PFX).");
        }

        return certificate;
    }
}

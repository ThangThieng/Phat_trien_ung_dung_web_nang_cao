using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CulinaryBlog.API.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace CulinaryBlog.API.IntegrationTests;

/// <summary>
/// Retrofit D-17 (Buổi 7): khóa DataProtection trên volume <c>dpkeys</c> phải ở dạng mã hóa ở production, và hai container
/// (<c>api</c>, <c>hangfire</c>) dùng chung thư mục khóa + chứng chỉ vẫn đọc được dữ liệu của nhau (NFR-SCALE-001, FR-AUTH-011).
/// Mỗi "container" ở đây là một ServiceProvider riêng — đúng như hai process không chia sẻ bộ nhớ.
/// </summary>
public sealed class DataProtectionKeyEncryptionTests : IDisposable
{
    private const string Password = "test-only-pfx-password";
    private const string Purpose = "D17.CrossContainer";
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("dpkeys-test-");

    private string KeysPath => Path.Combine(_root.FullName, "keys");

    private string CertificatePath => Path.Combine(_root.FullName, "dataprotection.pfx");

    [Fact]
    public void WithCertificate_KeyFilesOnDiskAreEncrypted_NotPlainText()
    {
        WriteCertificate();
        using var api = BuildProvider(Environments.Production, withCertificate: true);

        api.GetRequiredService<IDataProtectionProvider>().CreateProtector(Purpose).Protect("x");

        var keyFiles = Directory.GetFiles(KeysPath, "key-*.xml");
        var xml = File.ReadAllText(Assert.Single(keyFiles));
        Assert.Contains("encryptedSecret", xml, StringComparison.Ordinal);
        Assert.Contains("EncryptedXmlDecryptor", xml, StringComparison.Ordinal);

        // Khóa dạng rõ có <masterKey …><value>BASE64</value></masterKey>; bản mã hóa không còn phần tử này.
        Assert.DoesNotContain("<masterKey", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void SecondContainerWithSameKeysAndCertificate_UnprotectsWhatTheFirstProtected()
    {
        WriteCertificate();
        using var worker = BuildProvider(Environments.Production, withCertificate: true);
        using var api = BuildProvider(Environments.Production, withCertificate: true);

        var payload = worker.GetRequiredService<IDataProtectionProvider>().CreateProtector(Purpose).Protect("reset-token-payload");

        Assert.Equal(
            "reset-token-payload",
            api.GetRequiredService<IDataProtectionProvider>().CreateProtector(Purpose).Unprotect(payload));
    }

    [Fact]
    public void WithoutThePrivateKey_EncryptedKeyRingCannotBeRead()
    {
        WriteCertificate();
        using var api = BuildProvider(Environments.Production, withCertificate: true);
        var payload = api.GetRequiredService<IDataProtectionProvider>().CreateProtector(Purpose).Protect("secret");

        // Ai chỉ có bản sao volume mà không có chứng chỉ: không giải được khóa → không giải được dữ liệu.
        using var thief = BuildProvider(Environments.Development, withCertificate: false);

        Assert.ThrowsAny<CryptographicException>(
            () => thief.GetRequiredService<IDataProtectionProvider>().CreateProtector(Purpose).Unprotect(payload));
    }

    [Fact]
    public void Production_WithKeysPathButNoCertificate_RefusesToStart()
    {
        var error = Assert.Throws<InvalidOperationException>(() => BuildProvider(Environments.Production, withCertificate: false));

        Assert.Contains("DataProtection__CertificatePath", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Development_WithoutCertificate_KeepsWorkingLikeBefore()
    {
        using var api = BuildProvider(Environments.Development, withCertificate: false);
        var protector = api.GetRequiredService<IDataProtectionProvider>().CreateProtector(Purpose);

        Assert.Equal("x", protector.Unprotect(protector.Protect("x")));
        Assert.Single(Directory.GetFiles(KeysPath, "key-*.xml"));
    }

    [Fact]
    public void CertificateWithoutPrivateKey_IsRejected()
    {
        using var rsa = RSA.Create(2048);
        using var certificate = CreateCertificate(rsa);
        var publicOnly = Path.Combine(_root.FullName, "public-only.pfx");
        using (var withoutKey = X509CertificateLoader.LoadCertificate(certificate.Export(X509ContentType.Cert)))
        {
            File.WriteAllBytes(publicOnly, withoutKey.Export(X509ContentType.Pfx, Password));
        }

        Assert.Throws<InvalidOperationException>(() => DataProtectionExtensions.LoadCertificate(publicOnly, Password));
    }

    public void Dispose() => _root.Delete(recursive: true);

    private static X509Certificate2 CreateCertificate(RSA rsa)
    {
        var request = new CertificateRequest("CN=culinaryblog-dataprotection-test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, critical: false));
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddYears(1));
    }

    private void WriteCertificate()
    {
        using var rsa = RSA.Create(2048);
        using var certificate = CreateCertificate(rsa);
        File.WriteAllBytes(CertificatePath, certificate.Export(X509ContentType.Pfx, Password));
    }

    private ServiceProvider BuildProvider(string environmentName, bool withCertificate)
    {
        var values = new Dictionary<string, string?> { ["DataProtection:KeysPath"] = KeysPath };
        if (withCertificate)
        {
            values["DataProtection:CertificatePath"] = CertificatePath;
            values["DataProtection:CertificatePassword"] = Password;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCulinaryDataProtection(configuration, new HostEnvironment(environmentName));
        return services.BuildServiceProvider();
    }

    private sealed class HostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "CulinaryBlog.API";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

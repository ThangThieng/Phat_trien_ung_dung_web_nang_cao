using System.Collections.Concurrent;
using System.Text;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Auth;

namespace CulinaryBlog.API.IntegrationTests.Infrastructure.Fakes;

/// <summary>
/// Thay <see cref="IGoogleIdTokenValidator"/> trong integration test — không gọi Google thật (FR-AUTH-003). Test đăng ký
/// trước "ID token nào → kết quả nào"; cài đặt thật (GoogleIdTokenValidator) mới là nơi dịch lỗi thư viện, nên bản giả
/// ném đúng các lỗi nghiệp vụ mà cài đặt thật ném.
/// </summary>
public sealed class FakeGoogleIdTokenValidator : IGoogleIdTokenValidator
{
    /// <summary>Token giả có dạng JWT (3 đoạn) để vượt qua validator định dạng của GoogleLoginCommand.</summary>
    public static readonly string RejectedToken = CreateTestToken("rejected");

    public static readonly string UnavailableToken = CreateTestToken("unavailable");

    private readonly ConcurrentDictionary<string, GoogleIdTokenPayload> _payloads = new(StringComparer.Ordinal);

    /// <summary>Đăng ký một ID token hợp lệ; trả về chuỗi token để gửi lên /auth/google.</summary>
    public string Issue(GoogleIdTokenPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var token = CreateTestToken(Guid.NewGuid().ToString("N"));
        _payloads[token] = payload;
        return token;
    }

    public Task<GoogleIdTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        if (idToken == RejectedToken)
        {
            throw InvalidTokenException.GoogleTokenRejected();
        }

        if (idToken == UnavailableToken)
        {
            throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Không thể xác minh Google ID token lúc này.");
        }

        return _payloads.TryGetValue(idToken, out var payload)
            ? Task.FromResult(payload)
            : throw InvalidTokenException.GoogleTokenRejected();
    }

    private static string CreateTestToken(string signature) =>
        $"{Encode("{\"alg\":\"HS256\"}")}.{Encode("{}")}.{Encode(signature)}";

    private static string Encode(string value) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

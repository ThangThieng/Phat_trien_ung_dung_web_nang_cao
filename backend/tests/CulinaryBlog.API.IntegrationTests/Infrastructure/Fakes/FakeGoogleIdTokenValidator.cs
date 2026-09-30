using System.Collections.Concurrent;
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
    public const string RejectedToken = "google.rejected.token";

    public const string UnavailableToken = "google.unavailable.token";

    private readonly ConcurrentDictionary<string, GoogleIdTokenPayload> _payloads = new(StringComparer.Ordinal);

    /// <summary>Đăng ký một ID token hợp lệ; trả về chuỗi token để gửi lên /auth/google.</summary>
    public string Issue(GoogleIdTokenPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var token = $"google.{Guid.NewGuid():N}.token";
        _payloads[token] = payload;
        return token;
    }

    public Task<GoogleIdTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken) => idToken switch
    {
        RejectedToken => throw InvalidTokenException.GoogleTokenRejected(),
        UnavailableToken => throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Không thể xác minh Google ID token lúc này."),
        _ when _payloads.TryGetValue(idToken, out var payload) => Task.FromResult(payload),
        _ => throw InvalidTokenException.GoogleTokenRejected(),
    };
}

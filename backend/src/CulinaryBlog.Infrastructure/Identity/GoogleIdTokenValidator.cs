using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Auth;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// FR-AUTH-003 – xác minh Google ID Token bằng <c>GoogleJsonWebSignature.ValidateAsync</c>: thư viện tự kiểm chữ ký
/// (JWKS của Google), <c>iss</c>, <c>aud</c> (= ClientId của hệ thống) và <c>exp</c>. Đây là nơi DUY NHẤT dịch lỗi của
/// thư viện sang lỗi nghiệp vụ, nên handler chỉ thấy <see cref="InvalidTokenException"/> (401) hoặc
/// <see cref="BadGatewayException"/> (502).
/// </summary>
public sealed partial class GoogleIdTokenValidator(IOptions<GoogleAuthOptions> options, ILogger<GoogleIdTokenValidator> logger)
    : IGoogleIdTokenValidator
{
    public async Task<GoogleIdTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientId = options.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            // Đây là lỗi cấu hình phía máy chủ chứ không phải Google sập (review Buổi 3, mục 7), nên ghi log mức Error
            // nêu rõ khóa cấu hình cần đặt. Không chặn ứng dụng khởi động vì môi trường dev và test không dùng Google.
            // Người dùng vẫn nhận 502 AUTH_GOOGLE_UNAVAILABLE để giao diện hiện thông báo dự phòng theo SRS mục 2.6.2.
            LogMissingClientId(logger, GoogleAuthOptions.SectionName + ":ClientId");
            throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Đăng nhập Google tạm thời không khả dụng. Vui lòng đăng nhập bằng email.");
        }

        try
        {
            var payload = await GoogleJsonWebSignature
                .ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] })
                .ConfigureAwait(false);

            return new GoogleIdTokenPayload(payload.Subject, payload.Email, payload.EmailVerified, payload.Name, payload.Picture);
        }
        catch (InvalidJwtException ex)
        {
            throw InvalidTokenException.GoogleTokenRejected(ex);
        }
        catch (HttpRequestException)
        {
            throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Không thể xác minh Google ID token lúc này.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Không thể xác minh Google ID token lúc này.");
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Google sign-in is not configured: set {ConfigKey} (Google OAuth client ID)")]
    private static partial void LogMissingClientId(ILogger logger, string configKey);
}

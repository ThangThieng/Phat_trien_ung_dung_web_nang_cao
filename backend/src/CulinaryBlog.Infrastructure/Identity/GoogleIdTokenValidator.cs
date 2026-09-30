using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Auth;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// FR-AUTH-003 – xác minh Google ID Token bằng <c>GoogleJsonWebSignature.ValidateAsync</c>: thư viện tự kiểm chữ ký
/// (JWKS của Google), <c>iss</c>, <c>aud</c> (= ClientId của hệ thống) và <c>exp</c>. Đây là nơi DUY NHẤT dịch lỗi của
/// thư viện sang lỗi nghiệp vụ, nên handler chỉ thấy <see cref="InvalidTokenException"/> (401) hoặc
/// <see cref="BadGatewayException"/> (502).
/// </summary>
public sealed class GoogleIdTokenValidator(IOptions<GoogleAuthOptions> options) : IGoogleIdTokenValidator
{
    public async Task<GoogleIdTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientId = options.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Dịch vụ đăng nhập Google chưa được cấu hình.");
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
}

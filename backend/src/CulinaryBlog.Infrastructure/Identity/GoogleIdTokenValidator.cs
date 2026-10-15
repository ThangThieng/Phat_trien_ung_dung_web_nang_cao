using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions.Auth;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class GoogleIdTokenValidator(IOptions<GoogleAuthOptions> options, TimeProvider timeProvider) : IGoogleIdTokenValidator
{
    public async Task<VerifiedGoogleIdentity> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ClientId))
        {
            throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Dịch vụ đăng nhập Google chưa được cấu hình.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            // Google validates signature, issuer, audience and expiry; the configured client ID pins the audience.
            payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken, new GoogleJsonWebSignature.ValidationSettings { Audience = [options.Value.ClientId] })
                .ConfigureAwait(false);
        }
        catch (InvalidJwtException)
        {
            throw new InvalidTokenException("Google ID token không hợp lệ hoặc đã hết hạn.", ErrorCodes.AuthGoogleTokenInvalid);
        }
        catch (HttpRequestException)
        {
            throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Không thể xác minh Google ID token lúc này.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Không thể xác minh Google ID token lúc này.");
        }

        if (payload.Issuer is not ("accounts.google.com" or "https://accounts.google.com") ||
            payload.ExpirationTimeSeconds is null || payload.ExpirationTimeSeconds <= timeProvider.GetUtcNow().ToUnixTimeSeconds())
        {
            throw new InvalidTokenException("Google ID token không hợp lệ hoặc đã hết hạn.", ErrorCodes.AuthGoogleTokenInvalid);
        }

        return new VerifiedGoogleIdentity(payload.Subject, payload.Email, payload.Name, payload.Picture, payload.EmailVerified);
    }
}

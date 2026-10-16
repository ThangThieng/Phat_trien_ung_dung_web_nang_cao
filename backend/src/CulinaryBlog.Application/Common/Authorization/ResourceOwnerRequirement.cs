using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CulinaryBlog.Application.Common.Authorization;

/// <summary>
/// Resource-based authorization (NFR-SEC-006): "người gọi là chủ sở hữu tài nguyên HOẶC là Admin". Kiểm tra ở tầng
/// Application (không chỉ ở endpoint), mỗi loại tài nguyên một <see cref="AuthorizationHandler{TRequirement, TResource}"/>.
/// </summary>
public sealed class ResourceOwnerRequirement : IAuthorizationRequirement
{
    public static ResourceOwnerRequirement Instance { get; } = new();

    private ResourceOwnerRequirement()
    {
    }
}

public static class ResourceAuthorizationExtensions
{
    /// <summary>
    /// Chạy <see cref="ResourceOwnerRequirement"/> cho <paramref name="resource"/>; không đạt thì ném
    /// <see cref="ForbiddenException"/> với <paramref name="errorCode"/> (ví dụ RECIPE_FORBIDDEN → 403).
    /// </summary>
    public static async Task EnsureOwnerOrAdminAsync(
        this IAuthorizationService authorization,
        ICurrentUser currentUser,
        object resource,
        string errorCode,
        string message)
    {
        ArgumentNullException.ThrowIfNull(authorization);
        ArgumentNullException.ThrowIfNull(currentUser);

        var result = await authorization
            .AuthorizeAsync(currentUser.Principal, resource, ResourceOwnerRequirement.Instance)
            .ConfigureAwait(false);

        if (!result.Succeeded)
        {
            throw new ForbiddenException(errorCode, message);
        }
    }
}

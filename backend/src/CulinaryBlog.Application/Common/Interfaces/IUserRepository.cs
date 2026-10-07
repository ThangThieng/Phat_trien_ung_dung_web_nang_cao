using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;

namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>Read-only user queries; account writes remain behind IIdentityService/UserManager.</summary>
public interface IUserRepository
{
    Task<PagedResult<UserAdminDto>> GetUsersAsync(int page, int pageSize, string? search, bool? isActive, CancellationToken cancellationToken);
}

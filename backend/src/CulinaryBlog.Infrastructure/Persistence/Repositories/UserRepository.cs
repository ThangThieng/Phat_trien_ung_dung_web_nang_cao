using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(CulinaryBlogDbContext db) : IUserRepository
{
    public async Task<PagedResult<UserAdminDto>> GetUsersAsync(int page, int pageSize, string? search, bool? isActive, CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking();
        if (isActive.HasValue)
        {
            query = query.Where(user => user.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal);
            query = query.Where(user => EF.Functions.ILike(user.Email!, $"%{term}%", "\\") ||
                EF.Functions.ILike(user.DisplayName, $"%{term}%", "\\"));
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var users = await query.OrderByDescending(user => user.CreatedAt).ThenBy(user => user.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);
        var ids = users.Select(user => user.Id).ToArray();
        var counts = await db.Recipes.AsNoTracking().Where(recipe => ids.Contains(recipe.AuthorId))
            .GroupBy(recipe => recipe.AuthorId).Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken).ConfigureAwait(false);
        var roles = await (from membership in db.UserRoles.AsNoTracking()
                           join role in db.Roles.AsNoTracking() on membership.RoleId equals role.Id
                           where ids.Contains(membership.UserId)
                           select new { membership.UserId, role.Name }).ToListAsync(cancellationToken).ConfigureAwait(false);
        var rolesByUser = roles.ToLookup(role => role.UserId, role => role.Name!);
        var result = users.Select(user => new UserAdminDto(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.AvatarUrl,
            rolesByUser[user.Id].Order(StringComparer.Ordinal).ToArray(),
            user.IsActive,
            user.CreatedAt,
            counts.GetValueOrDefault(user.Id))).ToArray();
        return new PagedResult<UserAdminDto>(result, total, page, pageSize);
    }
}

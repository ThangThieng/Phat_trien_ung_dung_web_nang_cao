using CulinaryBlog.Application.Features.Auth;
using Microsoft.AspNetCore.Identity;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class UserNameGenerator(IUserRepository users, ILookupNormalizer normalizer) : IUserNameGenerator
{
    public async Task<string> GenerateAsync(string email, CancellationToken cancellationToken)
    {
        var prefix = email.Split('@')[0].Trim().ToLowerInvariant();
        const string allowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789-._@+";
        var baseName = new string(prefix.Where(character => allowedCharacters.Contains(character)).ToArray());
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "user";
        }

        baseName = baseName[..Math.Min(baseName.Length, 45)];

        var normalizedBase = normalizer.NormalizeName(baseName);
        var existing = await users.GetNormalizedUserNamesByPrefixAsync(normalizedBase, cancellationToken).ConfigureAwait(false);
        var used = existing.ToHashSet(StringComparer.Ordinal);
        var candidate = baseName;
        var suffix = 2;
        while (used.Contains(normalizer.NormalizeName(candidate)))
        {
            candidate = $"{baseName}{suffix++}";
        }

        return candidate;
    }
}

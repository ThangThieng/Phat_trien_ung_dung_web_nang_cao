using Microsoft.AspNetCore.OutputCaching;

namespace CulinaryBlog.API.Extensions;

/// <summary>
/// FR-RCP-001: policy "RecipeList" TTL 15 phút, vary theo query string.
/// FR-RCP-002: policy "RecipeDetail" TTL 60 phút, tag "recipes" + "recipe:{slug}" để invalidate theo tag (Buổi 4).
/// Policy mặc định của .NET không cache request có Authorization → Draft của tác giả không bị lộ qua cache.
/// </summary>
public static class OutputCachePolicies
{
    public const string RecipeList = "RecipeList";
    public const string RecipeDetail = "RecipeDetail";
    public const string RecipesTag = "recipes";

    public static IServiceCollection AddCulinaryOutputCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisOutputCache(o =>
        {
            o.Configuration = $"{configuration.GetConnectionString("Redis")},abortConnect=false,connectTimeout=2000";
            o.InstanceName = "culinaryblog:oc:";
        });

        services.AddOutputCache(options =>
        {
            options.AddPolicy(RecipeList, b => b.Expire(TimeSpan.FromMinutes(15)).SetVaryByQuery("*").Tag(RecipesTag));
            options.AddPolicy(RecipeDetail, b => b.Expire(TimeSpan.FromMinutes(60)).Tag(RecipesTag).AddPolicy<RecipeSlugTagPolicy>());
        });

        return services;
    }

    /// <summary>Gắn tag động "recipe:{slug}" cho từng response chi tiết.</summary>
    private sealed class RecipeSlugTagPolicy : IOutputCachePolicy
    {
        public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            if (context.HttpContext.Request.RouteValues.TryGetValue("slug", out var slug) && slug is string value)
            {
                context.Tags.Add($"recipe:{value}");
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;

        public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;
    }
}

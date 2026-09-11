using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>DbContext duy nhất (PostgreSQL 16, EF Core 10 Code-First) – đồng thời là Unit of Work.</summary>
public class CulinaryBlogDbContext(DbContextOptions<CulinaryBlogDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IUnitOfWork
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Recipe> Recipes => Set<Recipe>();

    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();

    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    public DbSet<RecipeImage> RecipeImages => Set<RecipeImage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // SRS §2.4.1: extension bắt buộc cho Full-Text Search tiếng Việt (FR-SRCH-001)
        builder.HasPostgresExtension("unaccent");
        builder.HasPostgresExtension("pg_trgm");

        builder.ApplyConfigurationsFromAssembly(typeof(CulinaryBlogDbContext).Assembly);
    }
}

using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

/// <summary>SRS §7.7 – custom columns của "AspNetUsers".</summary>
internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.Bio).HasColumnType("text");
        builder.Property(u => u.IsActive).HasDefaultValue(true);
        builder.Property(u => u.CreatedAt).HasColumnType("timestamptz").HasDefaultValueSql("NOW()");
    }
}

/// <summary>SRS §7.8 – "RefreshTokens".</summary>
internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()").ValueGeneratedNever();
        builder.Property(t => t.UserId).HasMaxLength(450).IsRequired();
        builder.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(t => t.ExpiresAt).HasColumnType("timestamptz");
        builder.Property(t => t.RevokedAt).HasColumnType("timestamptz");
        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(64);
        builder.Property(t => t.CreatedAt).HasColumnType("timestamptz").HasDefaultValueSql("NOW()");
        builder.Property(t => t.CreatedByIp).HasMaxLength(45);

        builder.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("IDX_RefreshToken_Hash");
        builder.HasIndex(t => t.UserId).HasDatabaseName("IDX_RefreshToken_UserId");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

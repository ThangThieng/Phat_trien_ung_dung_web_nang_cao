using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

/// <summary>SRS §7.6 – "Categories".</summary>
internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.ConfigureBase();

        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(120).IsRequired();
        builder.Property(c => c.Description).HasColumnType("text");
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.OrderIndex).HasDefaultValue(0);

        builder.HasIndex(c => c.Name).IsUnique().HasDatabaseName("IDX_Category_Name");
        builder.HasIndex(c => c.Slug).IsUnique().HasDatabaseName("IDX_Category_Slug");
    }
}

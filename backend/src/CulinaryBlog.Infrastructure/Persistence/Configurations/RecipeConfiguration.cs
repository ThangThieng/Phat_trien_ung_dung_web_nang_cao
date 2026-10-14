using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

/// <summary>SRS §7.2 – "Recipes" (Aggregate Root) + Owned Entity RecipeNutrition (§7.2.1).</summary>
internal sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    /// <summary>Cột <c>tsvector</c> của FR-SRCH-001 — shadow property: Domain chỉ dùng .NET BCL nên không mang kiểu Npgsql (CONS-001).</summary>
    public const string SearchVectorColumn = "SearchVector";

    /// <summary>Text search configuration duy nhất của hệ thống (MT-25): PostgreSQL 16 không có sẵn "vietnamese".</summary>
    public const string TextSearchConfig = "simple";

    /// <summary>
    /// SRS §7.2 — biểu thức generated column, dùng hàm wrapper IMMUTABLE <c>unaccent_immutable</c> do migration
    /// <c>B4_Search_FTS</c> tạo (unaccent() mặc định không IMMUTABLE nên không dùng trực tiếp được).
    /// </summary>
    public const string SearchVectorSql =
        "to_tsvector('simple', unaccent_immutable(coalesce(\"Title\",'') || ' ' || coalesce(\"Description\",'')))";

    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes", t =>
        {
            t.HasCheckConstraint("CK_Recipe_PrepTime", "\"PrepTimeMinutes\" > 0");
            t.HasCheckConstraint("CK_Recipe_CookTime", "\"CookTimeMinutes\" >= 0");
            t.HasCheckConstraint("CK_Recipe_Servings", "\"Servings\" > 0");
        });
        builder.ConfigureBase();

        builder.Property(r => r.Title).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Slug).HasMaxLength(220).IsRequired();
        builder.Property(r => r.Description).HasColumnType("text").IsRequired();
        builder.Property(r => r.Instructions).HasColumnType("text");
        builder.Property(r => r.Difficulty).HasConversion<short>().HasDefaultValue(RecipeDifficulty.Easy).HasSentinel((RecipeDifficulty)0);
        builder.Property(r => r.Status).HasConversion<short>().HasDefaultValue(RecipeStatus.Draft).HasSentinel((RecipeStatus)(-1));
        builder.Property(r => r.AuthorId).HasMaxLength(450).IsRequired();
        builder.Property(r => r.PublishedAt).HasColumnType("timestamptz");

        builder.OwnsOne(r => r.Nutrition, n =>
        {
            n.Property(x => x.Calories).HasColumnName("Nutrition_Calories").HasPrecision(8, 2);
            n.Property(x => x.Protein).HasColumnName("Nutrition_Protein").HasPrecision(8, 2);
            n.Property(x => x.Carbohydrates).HasColumnName("Nutrition_Carbohydrates").HasPrecision(8, 2);
            n.Property(x => x.Fat).HasColumnName("Nutrition_Fat").HasPrecision(8, 2);
            n.Property(x => x.Fiber).HasColumnName("Nutrition_Fiber").HasPrecision(8, 2);
            n.Property(x => x.Sodium).HasColumnName("Nutrition_Sodium").HasPrecision(8, 2);
        });

        // Category: ON DELETE RESTRICT – không xóa category còn recipe
        builder.HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Child collections – cascade delete (FR-RCP-007), truy cập qua backing field
        builder.HasMany(r => r.Steps).WithOne().HasForeignKey(s => s.RecipeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.Ingredients).WithOne().HasForeignKey(i => i.RecipeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.Images).WithOne().HasForeignKey(i => i.RecipeId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(r => r.Ingredients).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(r => r.Images).UsePropertyAccessMode(PropertyAccessMode.Field);

        // D-3 (MT-05, Buổi 4): unique CHỈ trên công thức chưa xóa mềm, để slug của bản đã xóa được dùng lại
        // thay vì chiếm chỗ vĩnh viễn như unique thường.
        builder.HasIndex(r => r.Slug).IsUnique().HasFilter("\"IsDeleted\" = false").HasDatabaseName("IDX_Recipe_Slug");
        builder.HasIndex(r => r.Status).HasDatabaseName("IDX_Recipe_Status");
        builder.HasIndex(r => r.CategoryId).HasDatabaseName("IDX_Recipe_CategoryId");
        builder.HasIndex(r => r.AuthorId).HasDatabaseName("IDX_Recipe_AuthorId");
        builder.HasIndex(r => r.Difficulty).HasDatabaseName("IDX_Recipe_Difficulty");
        builder.HasIndex(r => r.PublishedAt).HasDatabaseName("IDX_Recipe_PublishedAt");
        builder.HasIndex(r => r.CreatedAt).HasDatabaseName("IDX_Recipe_CreatedAt");
        builder.HasIndex(r => r.IsDeleted).HasDatabaseName("IDX_Recipe_IsDeleted").HasFilter("\"IsDeleted\" = false");

        // FR-SRCH-001 / MT-25 (Buổi 4 — Dev 3): generated column STORED, không trigger — PostgreSQL tự tính lại khi Title hoặc
        // Description đổi, không có đường code nào quên đồng bộ. GIN index cho toán tử @@.
        builder.Property<NpgsqlTsVector>(SearchVectorColumn)
            .HasColumnType("tsvector")
            .HasComputedColumnSql(SearchVectorSql, stored: true);
        builder.HasIndex(SearchVectorColumn).HasMethod("GIN").HasDatabaseName("IDX_Recipe_Search");
    }
}

/// <summary>SRS §7.3 – "RecipeSteps".</summary>
internal sealed class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        builder.ToTable("RecipeSteps", t =>
        {
            t.HasCheckConstraint("CK_RecipeStep_StepNumber", "\"StepNumber\" > 0");
            t.HasCheckConstraint("CK_RecipeStep_TimerMinutes", "\"TimerMinutes\" IS NULL OR \"TimerMinutes\" >= 0");
        });
        builder.ConfigureBase();

        builder.Property(s => s.Title).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasColumnType("text").IsRequired();
        builder.Property(s => s.ImageUrl).HasMaxLength(500);

        // D-16 (MT-56): tính duy nhất (RecipeId, StepNumber) là UNIQUE CONSTRAINT "UQ_RecipeStep_Recipe_StepNumber"
        // DEFERRABLE INITIALLY DEFERRED, tạo bằng DDL trong migration B4_Recipe_IngredientStep — PostgreSQL không cho unique
        // INDEX là deferrable, và EF Core không khai báo được constraint deferrable. Index ở đây chỉ phục vụ truy vấn.
        builder.HasIndex(s => new { s.RecipeId, s.StepNumber }).HasDatabaseName("IDX_RecipeStep_Recipe_StepNumber");
    }
}

/// <summary>SRS §7.4 – "RecipeIngredients".</summary>
internal sealed class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.ToTable("RecipeIngredients", t =>
            t.HasCheckConstraint("CK_RecipeIngredient_Quantity", "\"Quantity\" IS NULL OR \"Quantity\" > 0"));
        builder.ConfigureBase();

        builder.Property(i => i.Name).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Quantity).HasPrecision(10, 3);
        builder.Property(i => i.QuantityText).HasMaxLength(50);
        builder.Property(i => i.Unit).HasMaxLength(50);
        builder.Property(i => i.Notes).HasMaxLength(500);
        builder.Property(i => i.OrderIndex).HasDefaultValue(0);

        builder.HasIndex(i => i.RecipeId).HasDatabaseName("IDX_RecipeIngredient_RecipeId");
    }
}

/// <summary>SRS §7.5 – "RecipeImages". Partial unique index đảm bảo chỉ 1 ảnh IsPrimary / Recipe.</summary>
internal sealed class RecipeImageConfiguration : IEntityTypeConfiguration<RecipeImage>
{
    public void Configure(EntityTypeBuilder<RecipeImage> builder)
    {
        builder.ToTable("RecipeImages");
        builder.ConfigureBase();

        builder.Property(i => i.OriginalUrl).HasMaxLength(500).IsRequired();
        builder.Property(i => i.MediumUrl).HasMaxLength(500);
        builder.Property(i => i.ThumbnailUrl).HasMaxLength(500);
        builder.Property(i => i.AltText).HasMaxLength(200);
        builder.Property(i => i.IsPrimary).HasDefaultValue(false);
        builder.Property(i => i.OrderIndex).HasDefaultValue(0);

        builder.HasIndex(i => i.RecipeId, "IDX_RecipeImage_RecipeId");
        builder.HasIndex(i => i.RecipeId, "IDX_RecipeImage_Primary")
            .IsUnique()
            .HasFilter("\"IsPrimary\" = true AND \"IsDeleted\" = false");
    }
}

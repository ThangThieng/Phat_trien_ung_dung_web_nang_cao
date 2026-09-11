using CulinaryBlog.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

/// <summary>SRS §7.1 – cột kế thừa cho mọi bảng: Id uuid, CreatedAt, UpdatedAt, IsDeleted (global filter), RowVersion bytea.</summary>
internal static class BaseEntityConfiguration
{
    public static void ConfigureBase<T>(this EntityTypeBuilder<T> builder)
        where T : BaseEntity
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").ValueGeneratedNever();
        builder.Property(e => e.CreatedAt).HasColumnType("timestamptz").HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.Property(e => e.RowVersion).HasColumnType("bytea").IsRequired().IsConcurrencyToken();
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

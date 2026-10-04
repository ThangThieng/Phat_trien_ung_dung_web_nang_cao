using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

/// <summary>
/// FR-SRCH-001 / MT-25 / retrofit D-18 (Buổi 4 — Dev 3): một cơ chế tìm kiếm toàn văn duy nhất, khai báo TRỌN trong migration —
/// Testcontainers và môi trường mới không chạy <c>docker/postgres/init.sql</c>, nên mọi đối tượng schema mà code phụ thuộc phải
/// sinh ra từ đây. Thứ tự bắt buộc: extension → hàm <c>unaccent_immutable</c> → cột generated (tham chiếu hàm) → GIN index.
/// Cột generated tự tính cho mọi công thức đã có (100 công thức mẫu — CR-2026-03) ngay khi migration chạy, không cần backfill.
/// Migration viết tay (tiền lệ B3) nên tự khai báo [DbContext] + [Migration].
/// </summary>
[DbContext(typeof(CulinaryBlogDbContext))]
[Migration("20260930120000_B4_Search_FTS")]
public partial class B4_Search_FTS : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Idempotent: B1 và init.sql đã tạo trong Docker, nhưng migration không được giả định điều đó (SRS §7.2).
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

        // unaccent() mặc định KHÔNG immutable → không dùng trực tiếp trong generated column; bọc bằng wrapper IMMUTABLE (SRS §7.2).
        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION unaccent_immutable(text)
            RETURNS text AS $$ SELECT unaccent('unaccent', $1) $$
            LANGUAGE sql IMMUTABLE STRICT;
            """);

        migrationBuilder.AddColumn<NpgsqlTsVector>(
            name: "SearchVector",
            table: "Recipes",
            type: "tsvector",
            nullable: true,
            computedColumnSql: "to_tsvector('simple', unaccent_immutable(coalesce(\"Title\",'') || ' ' || coalesce(\"Description\",'')))",
            stored: true);

        migrationBuilder.CreateIndex(
                name: "IDX_Recipe_Search",
                table: "Recipes",
                column: "SearchVector")
            .Annotation("Npgsql:IndexMethod", "GIN");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IDX_Recipe_Search", table: "Recipes");
        migrationBuilder.DropColumn(name: "SearchVector", table: "Recipes");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS unaccent_immutable(text);");

        // Không DROP EXTENSION: unaccent/pg_trgm thuộc migration B1 (HasPostgresExtension), không thuộc migration này.
    }
}

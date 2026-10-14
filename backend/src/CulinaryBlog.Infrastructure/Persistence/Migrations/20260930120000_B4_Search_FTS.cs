using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

/// <summary>FR-SRCH-001: generated tsvector, immutable unaccent wrapper và GIN index.</summary>
[DbContext(typeof(CulinaryBlogDbContext))]
[Migration("20260930120000_B4_Search_FTS")]
public partial class B4_Search_FTS : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        migrationBuilder.Sql("""
            CREATE OR REPLACE FUNCTION unaccent_immutable(text)
            RETURNS text AS $$ SELECT unaccent('unaccent', $1) $$
            LANGUAGE sql IMMUTABLE STRICT;
            """);
        migrationBuilder.AddColumn<NpgsqlTsVector>(
            name: "SearchVector", table: "Recipes", type: "tsvector", nullable: true,
            computedColumnSql: "to_tsvector('simple', unaccent_immutable(coalesce(\"Title\",'') || ' ' || coalesce(\"Description\",'')))",
            stored: true);
        migrationBuilder.CreateIndex(name: "IDX_Recipe_Search", table: "Recipes", column: "SearchVector")
            .Annotation("Npgsql:IndexMethod", "GIN");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IDX_Recipe_Search", table: "Recipes");
        migrationBuilder.DropColumn(name: "SearchVector", table: "Recipes");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS unaccent_immutable(text);");
    }
}

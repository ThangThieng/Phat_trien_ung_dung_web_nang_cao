using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

/// <summary>
/// Buổi 7 (Dev 4 — hạ tầng production): bản sao lưu <c>pg_dump</c> của cơ sở dữ liệu không khôi phục được. Hàm
/// <c>unaccent_immutable</c> (B4_Search_FTS, D-18) gọi <c>unaccent('unaccent', $1)</c> không kèm schema; khi khôi phục,
/// <c>pg_dump</c> đặt <c>search_path</c> rỗng nên lời gọi bên trong hàm không tìm thấy <c>unaccent</c> → cột generated
/// <c>SearchVector</c> không tính được → bảng <c>Recipes</c> và mọi bảng tham chiếu tới nó không được tạo (đo 21/10/2026 trên bản
/// sao của cơ sở dữ liệu dev). Tạo lại hàm với tên đầy đủ schema (cách tài liệu PostgreSQL khuyến nghị cho wrapper của
/// <c>unaccent</c>): kết quả của hàm không đổi nên giá trị <c>SearchVector</c> và GIN index giữ nguyên, không phải tính lại. Không
/// dùng <c>SET search_path</c> trên hàm vì như vậy PostgreSQL không inline được hàm SQL này.
/// Migration viết tay (tiền lệ B3, B4, B5) nên tự khai báo [DbContext] + [Migration]; không đổi model nên snapshot giữ nguyên.
/// </summary>
[DbContext(typeof(CulinaryBlogDbContext))]
[Migration("20261021090000_B7_Search_UnaccentSchemaQualified")]
public partial class B7_Search_UnaccentSchemaQualified : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION public.unaccent_immutable(text)
            RETURNS text AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, $1) $$
            LANGUAGE sql IMMUTABLE STRICT;
            """);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION public.unaccent_immutable(text)
            RETURNS text AS $$ SELECT unaccent('unaccent', $1) $$
            LANGUAGE sql IMMUTABLE STRICT;
            """);
}

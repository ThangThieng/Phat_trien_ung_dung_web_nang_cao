using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

/// <summary>
/// CR-2026-08 / MT-71: tìm "pho" trả "Welsh Rarebit" (có "phô mai") trên "Phở bò". Hai nguyên nhân: Title và Description cùng trọng số,
/// và bỏ dấu làm "phô", "phổ", "phố" trùng với "phở". Sửa: Title trọng số A, Description trọng số B; hàm <c>search_normalize</c> gộp các
/// từ ghép thường gặp thành một token bắt đầu bằng "q" (khi còn dấu, nên "phở biển" không bị gộp nhầm với "phổ biến"), để truy vấn
/// tiền tố <c>pho:*</c> không khớp. <see cref="Application.Features.Recipes.SearchTermBuilder"/> ánh xạ từ khóa sang đúng các token đó.
/// Cột generated không đổi được biểu thức nên phải xóa rồi tạo lại; PostgreSQL tự tính lại cho mọi dòng. Không đổi API.
/// Migration viết tay (tiền lệ B3, B4, B5, B7) nên tự khai báo [DbContext] + [Migration].
/// </summary>
[DbContext(typeof(CulinaryBlogDbContext))]
[Migration("20261104090000_B8_Search_TitleWeightCompoundTerms")]
public partial class B8_Search_TitleWeightCompoundTerms : Migration
{
    private const string OldExpression =
        """to_tsvector('simple', unaccent_immutable(coalesce("Title",'') || ' ' || coalesce("Description",'')))""";

    private const string NewExpression =
        """setweight(to_tsvector('simple', public.search_normalize(coalesce("Title",''))), 'A') || setweight(to_tsvector('simple', public.search_normalize(coalesce("Description",''))), 'B')""";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION public.search_normalize(text)
            RETURNS text AS $$
              SELECT public.unaccent_immutable(
                regexp_replace(regexp_replace(regexp_replace(regexp_replace(
                regexp_replace(regexp_replace(regexp_replace(regexp_replace(
                  lower($1),
                  '(^|[^[:alpha:]])ph[ôo][[:space:]-]*m(ai|át|at)(?=$|[^[:alpha:]])', '\1qphomai', 'g'),
                  '(^|[^[:alpha:]])phổ[[:space:]-]+biến(?=$|[^[:alpha:]])', '\1qphobien', 'g'),
                  '(^|[^[:alpha:]])phổ[[:space:]-]+thông(?=$|[^[:alpha:]])', '\1qphothong', 'g'),
                  '(^|[^[:alpha:]])thành[[:space:]-]+phố(?=$|[^[:alpha:]])', '\1qthanhpho', 'g'),
                  '(^|[^[:alpha:]])đường[[:space:]-]+phố(?=$|[^[:alpha:]])', '\1qduongpho', 'g'),
                  '(^|[^[:alpha:]])khu[[:space:]-]+phố(?=$|[^[:alpha:]])', '\1qkhupho', 'g'),
                  '(^|[^[:alpha:]])phố[[:space:]-]+cổ(?=$|[^[:alpha:]])', '\1qphoco', 'g'),
                  '(^|[^[:alpha:]])phố[[:space:]-]+hội(?=$|[^[:alpha:]])', '\1qphohoi', 'g'))
            $$ LANGUAGE sql IMMUTABLE STRICT;
            """);
        ReplaceColumn(migrationBuilder, NewExpression);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ReplaceColumn(migrationBuilder, OldExpression);
        migrationBuilder.Sql("DROP FUNCTION public.search_normalize(text);");
    }

    private static void ReplaceColumn(MigrationBuilder migrationBuilder, string expression)
    {
        migrationBuilder.Sql("""DROP INDEX "IDX_Recipe_Search";""");
        migrationBuilder.Sql("""ALTER TABLE "Recipes" DROP COLUMN "SearchVector";""");
        migrationBuilder.Sql($"""ALTER TABLE "Recipes" ADD COLUMN "SearchVector" tsvector GENERATED ALWAYS AS ({expression}) STORED;""");
        migrationBuilder.Sql("""CREATE INDEX "IDX_Recipe_Search" ON "Recipes" USING GIN ("SearchVector");""");
    }
}

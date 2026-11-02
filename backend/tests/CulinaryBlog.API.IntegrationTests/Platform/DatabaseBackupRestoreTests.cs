using System.Globalization;
using CulinaryBlog.API.IntegrationTests.Infrastructure;

namespace CulinaryBlog.API.IntegrationTests;

/// <summary>
/// Buổi 7: bản sao lưu <c>pg_dump</c> của cơ sở dữ liệu (đúng schema mà các migration sinh ra) phải khôi phục được vào một cơ sở
/// dữ liệu trống, và tìm kiếm toàn văn trên bản khôi phục cho cùng kết quả. Trước migration B7_Search_UnaccentSchemaQualified,
/// hàm <c>unaccent_immutable</c> gọi <c>unaccent</c> không kèm schema nên bước khôi phục (search_path rỗng) hỏng ở bảng Recipes.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class DatabaseBackupRestoreTests(CulinaryBlogApiFactory factory)
{
    private const string Script =
        """
        set -e
        dropdb -U culinary --if-exists restore_check
        createdb -U culinary -T template0 restore_check
        pg_dump -U culinary culinaryblog_tests | psql -v ON_ERROR_STOP=1 -q -U culinary -d restore_check >/dev/null
        q="SELECT count(*) FROM \"Recipes\" WHERE \"SearchVector\" @@ to_tsquery('simple', public.unaccent_immutable('Hà & Nội'))"
        echo "$(psql -At -U culinary -d culinaryblog_tests -c "$q") $(psql -At -U culinary -d restore_check -c "$q")"
        dropdb -U culinary restore_check
        """;

    [Fact]
    public async Task PgDump_RestoresIntoEmptyDatabase_WithWorkingFullTextSearch()
    {
        var result = await factory.ExecInPostgresAsync(["sh", "-c", Script]);

        Assert.True(result.ExitCode == 0, $"Khôi phục bản pg_dump thất bại (mã {result.ExitCode}): {result.Stderr}");
        var counts = result.Stdout.Trim().Split(' ').Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();
        Assert.Equal(2, counts.Length);
        Assert.True(counts[0] > 0, "Bộ dữ liệu test phải có công thức khớp 'Hà Nội'.");
        Assert.Equal(counts[0], counts[1]);
    }
}

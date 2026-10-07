using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

/// <summary>
/// NFR-PERF-004 / MT-26 (Buổi 5 — Dev 3): ba composite index bắt buộc của SRS §7.2. Buổi 2 chỉ có index đơn cột; chưa buổi nào tạo
/// ba index này. Chọn theo hình dạng truy vấn thật: mọi truy vấn danh sách đều lọc <c>Status = Published</c> (và Global Query Filter
/// <c>IsDeleted = false</c>) rồi mới sắp xếp, nên cột lọc đứng trước cột sắp xếp. Kết quả đo ở <c>docs/explain-analyze-b5.md</c>.
/// Migration viết tay (tiền lệ B3, B4) nên tự khai báo [DbContext] + [Migration].
/// </summary>
[DbContext(typeof(CulinaryBlogDbContext))]
[Migration("20261007180000_B5_Search_CompositeIndexes")]
public partial class B5_Search_CompositeIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // FR-RCP-001 (danh sách công khai), trang chủ sắp xếp theo publishedAt
        migrationBuilder.CreateIndex(
            name: "IDX_Recipe_List",
            table: "Recipes",
            columns: new[] { "IsDeleted", "Status", "PublishedAt" },
            descending: new[] { false, false, true });

        // FR-CAT-002 (công thức theo danh mục), bộ lọc categoryId
        migrationBuilder.CreateIndex(
            name: "IDX_Recipe_ByCategory",
            table: "Recipes",
            columns: new[] { "Status", "CategoryId", "PublishedAt" },
            descending: new[] { false, false, true });

        // FR-SRCH-002 maxCookTime, FR-RCP-001 sắp xếp cookTime
        migrationBuilder.CreateIndex(
            name: "IDX_Recipe_CookTime",
            table: "Recipes",
            columns: new[] { "Status", "CookTimeMinutes" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IDX_Recipe_CookTime", table: "Recipes");
        migrationBuilder.DropIndex(name: "IDX_Recipe_ByCategory", table: "Recipes");
        migrationBuilder.DropIndex(name: "IDX_Recipe_List", table: "Recipes");
    }
}

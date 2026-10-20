using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

/// <summary>
/// D-8 / FR-RCP-003: Instructions là trường tùy chọn; bước nấu được quản lý riêng ở FR-RCP-010.
/// Migration viết tay nên phải tự khai báo [DbContext] + [Migration] (bình thường nằm ở file .Designer.cs do EF sinh) —
/// thiếu hai attribute này thì EF không phát hiện migration và cột trong DB vẫn NOT NULL dù model snapshot đã đổi.
/// </summary>
[DbContext(typeof(CulinaryBlogDbContext))]
[Migration("20260928084500_B3_Recipe_InstructionsNullable")]
public partial class B3_Recipe_InstructionsNullable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AlterColumn<string>(
        name: "Instructions", table: "Recipes", type: "text", nullable: true, oldClrType: typeof(string), oldType: "text");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.AlterColumn<string>(
        name: "Instructions", table: "Recipes", type: "text", nullable: false, defaultValue: string.Empty, oldClrType: typeof(string), oldType: "text", oldNullable: true);
}

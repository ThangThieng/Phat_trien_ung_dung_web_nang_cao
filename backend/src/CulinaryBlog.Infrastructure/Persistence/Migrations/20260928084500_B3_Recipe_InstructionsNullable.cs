using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

/// <summary>FR-RCP-003: Instructions là trường tùy chọn; bước nấu sẽ được quản lý riêng ở FR-RCP-010.</summary>
public partial class B3_Recipe_InstructionsNullable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AlterColumn<string>(
        name: "Instructions", table: "Recipes", type: "text", nullable: true, oldClrType: typeof(string), oldType: "text");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.AlterColumn<string>(
        name: "Instructions", table: "Recipes", type: "text", nullable: false, defaultValue: string.Empty, oldClrType: typeof(string), oldType: "text", oldNullable: true);
}

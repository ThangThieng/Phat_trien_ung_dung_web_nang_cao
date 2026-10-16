using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Buổi 4 — Dev 2: FR-RCP-009 (cột QuantityText + CHECK Quantity &gt; 0) và retrofit D-16 (MT-56): tính duy nhất
    /// (RecipeId, StepNumber) chuyển từ unique INDEX sang unique CONSTRAINT DEFERRABLE INITIALLY DEFERRED — PostgreSQL
    /// kiểm tra unique index ngay sau TỪNG câu UPDATE và không cho index là deferrable, nên hoán đổi hai bước luôn nổ 23505.
    /// EF Core không khai báo được constraint deferrable → DDL viết tay (hợp lệ theo CONS-006, MT-25).
    /// </summary>
    public partial class B4_Recipe_IngredientStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_RecipeStep_Recipe_StepNumber",
                table: "RecipeSteps");

            migrationBuilder.AddColumn<string>(
                name: "QuantityText",
                table: "RecipeIngredients",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IDX_RecipeStep_Recipe_StepNumber",
                table: "RecipeSteps",
                columns: new[] { "RecipeId", "StepNumber" });

            // Bất biến mới "không rỗng cả Quantity, QuantityText lẫn Unit" (FR-RCP-009): nguyên liệu cũ để trống cả ba
            // là loại "vừa đủ / ăn kèm" — đưa phần định lượng dạng chữ vào QuantityText, khớp bộ seed đã sửa cùng buổi.
            migrationBuilder.Sql("""
                UPDATE "RecipeIngredients"
                SET "QuantityText" = CASE WHEN "Notes" = 'nêm vừa ăn' THEN 'vừa ăn' ELSE 'vừa đủ' END,
                    "Notes" = CASE "Notes" WHEN 'vừa đủ' THEN NULL WHEN 'nêm vừa ăn' THEN 'dùng để nêm' ELSE "Notes" END
                WHERE "Quantity" IS NULL AND "QuantityText" IS NULL AND "Unit" IS NULL;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RecipeIngredient_Quantity",
                table: "RecipeIngredients",
                sql: "\"Quantity\" IS NULL OR \"Quantity\" > 0");

            migrationBuilder.Sql("""
                ALTER TABLE "RecipeSteps" ADD CONSTRAINT "UQ_RecipeStep_Recipe_StepNumber"
                    UNIQUE ("RecipeId", "StepNumber") DEFERRABLE INITIALLY DEFERRED;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "RecipeSteps" DROP CONSTRAINT "UQ_RecipeStep_Recipe_StepNumber";""");

            migrationBuilder.DropIndex(
                name: "IDX_RecipeStep_Recipe_StepNumber",
                table: "RecipeSteps");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RecipeIngredient_Quantity",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "QuantityText",
                table: "RecipeIngredients");

            migrationBuilder.CreateIndex(
                name: "IDX_RecipeStep_Recipe_StepNumber",
                table: "RecipeSteps",
                columns: new[] { "RecipeId", "StepNumber" },
                unique: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

/// <summary>Buổi 4: định lượng tự do và ràng buộc hoãn kiểm tra khi đổi thứ tự bước.</summary>
public partial class B4_Recipe_IngredientStep : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "QuantityText", table: "RecipeIngredients", type: "character varying(50)", maxLength: 50, nullable: true);

        migrationBuilder.AddCheckConstraint(
            name: "CK_RecipeIngredient_QuantityPositive", table: "RecipeIngredients",
            sql: "\"Quantity\" IS NULL OR \"Quantity\" > 0");

        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IDX_RecipeStep_Recipe_StepNumber\";");
        migrationBuilder.Sql("ALTER TABLE \"RecipeSteps\" ADD CONSTRAINT \"UQ_RecipeStep_Recipe_StepNumber\" UNIQUE (\"RecipeId\", \"StepNumber\") DEFERRABLE INITIALLY DEFERRED;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"RecipeSteps\" DROP CONSTRAINT IF EXISTS \"UQ_RecipeStep_Recipe_StepNumber\";");
        migrationBuilder.CreateIndex(name: "IDX_RecipeStep_Recipe_StepNumber", table: "RecipeSteps", columns: new[] { "RecipeId", "StepNumber" }, unique: true);
        migrationBuilder.DropCheckConstraint(name: "CK_RecipeIngredient_QuantityPositive", table: "RecipeIngredients");
        migrationBuilder.DropColumn(name: "QuantityText", table: "RecipeIngredients");
    }
}

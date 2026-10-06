using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class B4_Recipe_PartialUniqueSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_Recipe_Slug",
                table: "Recipes");

            migrationBuilder.CreateIndex(
                name: "IDX_Recipe_Slug",
                table: "Recipes",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_Recipe_Slug",
                table: "Recipes");

            migrationBuilder.CreateIndex(
                name: "IDX_Recipe_Slug",
                table: "Recipes",
                column: "Slug",
                unique: true);
        }
    }
}

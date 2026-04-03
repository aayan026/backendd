using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FurnitureShop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init79 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_CollectionCategories_CollectionCategoryId",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_FurnitureCategories_FurnitureCategoryId",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_CollectionCategories_CollectionCategoryId",
                table: "Collections",
                column: "CollectionCategoryId",
                principalTable: "CollectionCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_FurnitureCategories_FurnitureCategoryId",
                table: "Products",
                column: "FurnitureCategoryId",
                principalTable: "FurnitureCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_CollectionCategories_CollectionCategoryId",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_FurnitureCategories_FurnitureCategoryId",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_CollectionCategories_CollectionCategoryId",
                table: "Collections",
                column: "CollectionCategoryId",
                principalTable: "CollectionCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_FurnitureCategories_FurnitureCategoryId",
                table: "Products",
                column: "FurnitureCategoryId",
                principalTable: "FurnitureCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

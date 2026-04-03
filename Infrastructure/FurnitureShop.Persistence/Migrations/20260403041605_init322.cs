using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FurnitureShop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init322 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "producCount",
                table: "FurnitureCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "collectionCount",
                table: "CollectionCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "producCount",
                table: "FurnitureCategories");

            migrationBuilder.DropColumn(
                name: "collectionCount",
                table: "CollectionCategories");
        }
    }
}

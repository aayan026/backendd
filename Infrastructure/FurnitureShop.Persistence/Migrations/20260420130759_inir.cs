using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FurnitureShop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class inir : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                table: "HeroSections",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeroSections_CollectionId",
                table: "HeroSections",
                column: "CollectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_HeroSections_Collections_CollectionId",
                table: "HeroSections",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HeroSections_Collections_CollectionId",
                table: "HeroSections");

            migrationBuilder.DropIndex(
                name: "IX_HeroSections_CollectionId",
                table: "HeroSections");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "HeroSections");
        }
    }
}

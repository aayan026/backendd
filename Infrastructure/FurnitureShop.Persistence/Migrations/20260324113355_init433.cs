using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FurnitureShop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init433 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Collections",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Collections");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FurnitureShop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "Orders",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomDescription",
                table: "Orders",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstallmentMonths",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomOrder",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPayment",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TimeSlot",
                table: "DeliveryInfos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ScheduledDate",
                table: "DeliveryInfos",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomDescription",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InstallmentMonths",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsCustomOrder",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MonthlyPayment",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "TimeSlot",
                table: "DeliveryInfos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ScheduledDate",
                table: "DeliveryInfos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}

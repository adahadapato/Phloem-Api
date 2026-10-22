using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Phloem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ShopCutoffAndDecline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "OrderCutoffTime",
                table: "Vendors",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(19, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "DeclineReason",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeclinedByShop",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderCutoffTime",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "DeclineReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeclinedByShop",
                table: "Orders");
        }
    }
}

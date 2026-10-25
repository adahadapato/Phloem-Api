using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Phloem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddressRecipient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryPhone",
                table: "Orders",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryRecipientName",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "DeliveryAddresses",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "DeliveryAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryPhone",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryRecipientName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "DeliveryAddresses");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "DeliveryAddresses");
        }
    }
}

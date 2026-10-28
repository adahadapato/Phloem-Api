using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Phloem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCouriersAndProofOfDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CourierAssignedAtUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourierId",
                table: "Shipments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourierName",
                table: "Shipments",
                type: "nvarchar(101)",
                maxLength: 101,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryProof_CapturedAtUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryProof_CourierId",
                table: "Shipments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryProof_Handover",
                table: "Shipments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryProof_Note",
                table: "Shipments",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryProof_PhotoPath",
                table: "Shipments",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryProof_ReceivedBy",
                table: "Shipments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CourierAssignedAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourierId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourierName",
                table: "Orders",
                type: "nvarchar(101)",
                maxLength: 101,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryProof_CapturedAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryProof_CourierId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryProof_Handover",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryProof_Note",
                table: "Orders",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryProof_PhotoPath",
                table: "Orders",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryProof_ReceivedBy",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_CourierId_Status",
                table: "Shipments",
                columns: new[] { "CourierId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CourierId_Status",
                table: "Orders",
                columns: new[] { "CourierId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_UserProfiles_CourierId",
                table: "Orders",
                column: "CourierId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_UserProfiles_CourierId",
                table: "Shipments",
                column: "CourierId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_UserProfiles_CourierId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_UserProfiles_CourierId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_CourierId_Status",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CourierId_Status",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CourierAssignedAtUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CourierId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CourierName",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_CapturedAtUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_CourierId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_Handover",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_Note",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_PhotoPath",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_ReceivedBy",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CourierAssignedAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CourierId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CourierName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_CapturedAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_CourierId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_Handover",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_Note",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_PhotoPath",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryProof_ReceivedBy",
                table: "Orders");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Phloem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderChats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopOwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Country = table.Column<int>(type: "int", nullable: false),
                    LastCustomerMessageAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSellerMessageAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerReadUpToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SellerReadUpToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerEmailedUpToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SellerEmailedUpToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReported = table.Column<bool>(type: "bit", nullable: false),
                    ReportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReportedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReportReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportHandledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReportHandledBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderChats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderRole = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HiddenBySender = table.Column<bool>(type: "bit", nullable: false),
                    HasContactDetails = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_OrderChats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "OrderChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatId_CreatedAt",
                table: "ChatMessages",
                columns: new[] { "ChatId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderChats_CustomerId_LastCustomerMessageAtUtc",
                table: "OrderChats",
                columns: new[] { "CustomerId", "LastCustomerMessageAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderChats_IsReported_ReportHandledAtUtc",
                table: "OrderChats",
                columns: new[] { "IsReported", "ReportHandledAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderChats_OrderId",
                table: "OrderChats",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderChats_ShopOwnerId_LastSellerMessageAtUtc",
                table: "OrderChats",
                columns: new[] { "ShopOwnerId", "LastSellerMessageAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "OrderChats");
        }
    }
}

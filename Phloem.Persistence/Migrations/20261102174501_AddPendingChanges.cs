using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Phloem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Country = table.Column<int>(type: "int", nullable: true),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProposedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecidedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DecidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecisionNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingChanges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingChanges_CreatedAt",
                table: "PendingChanges",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PendingChanges_Kind_TargetId_Status",
                table: "PendingChanges",
                columns: new[] { "Kind", "TargetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingChanges_ProposedById_CreatedAt",
                table: "PendingChanges",
                columns: new[] { "ProposedById", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingChanges_Status_ExpiresAtUtc",
                table: "PendingChanges",
                columns: new[] { "Status", "ExpiresAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingChanges");
        }
    }
}

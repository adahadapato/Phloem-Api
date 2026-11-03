using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Phloem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "Dishes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "Dishes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "ModerationChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Country = table.Column<int>(type: "int", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    AiVerdict = table.Column<int>(type: "int", nullable: true),
                    AiReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AiLabel = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IsSpam = table.Column<bool>(type: "bit", nullable: true),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: true),
                    AiCallId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AiAttempts = table.Column<int>(type: "int", nullable: false),
                    StaffVerdict = table.Column<int>(type: "int", nullable: true),
                    StaffNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DecidedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DecidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationChecks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationChecks_Kind_TargetId_CreatedAt",
                table: "ModerationChecks",
                columns: new[] { "Kind", "TargetId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationChecks_State_CreatedAt",
                table: "ModerationChecks",
                columns: new[] { "State", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationChecks_State_Kind_Country",
                table: "ModerationChecks",
                columns: new[] { "State", "Kind", "Country" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModerationChecks");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "Dishes");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "Dishes");
        }
    }
}

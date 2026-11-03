using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Phloem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSEO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeoEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    ShopId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(170)", maxLength: 170, nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Improvements = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    ScoreExplanation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Source = table.Column<int>(type: "int", nullable: false),
                    SourceFingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    AiCallId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeoEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeoEntries_Key",
                table: "SeoEntries",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeoEntries");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Phloem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserSuspension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOwnerSuspended",
                table: "Vendors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedAtUtc",
                table: "UserProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SuspendedByUserId",
                table: "UserProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedUntilUtc",
                table: "UserProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "UserProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_SuspendedAtUtc",
                table: "UserProfiles",
                column: "SuspendedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_SuspendedAtUtc",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsOwnerSuspended",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "SuspendedAtUtc",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SuspendedByUserId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SuspendedUntilUtc",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "UserProfiles");
        }
    }
}

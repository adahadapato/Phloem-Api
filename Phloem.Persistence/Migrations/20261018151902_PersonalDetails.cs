using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Phloem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersonalDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "UserProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidentialAddress_City",
                table: "UserProfiles",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResidentialAddress_Country",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidentialAddress_Line1",
                table: "UserProfiles",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidentialAddress_Line2",
                table: "UserProfiles",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidentialAddress_RegionOrPostcode",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ResidentialAddress_City",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ResidentialAddress_Country",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ResidentialAddress_Line1",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ResidentialAddress_Line2",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ResidentialAddress_RegionOrPostcode",
                table: "UserProfiles");
        }
    }
}

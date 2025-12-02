using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LolStatsTracker.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRankAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentDivision",
                table: "Matches",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentLp",
                table: "Matches",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CurrentTier",
                table: "Matches",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LpChange",
                table: "Matches",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Matches",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentDivision",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "CurrentLp",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "CurrentTier",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LpChange",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Matches");
        }
    }
}

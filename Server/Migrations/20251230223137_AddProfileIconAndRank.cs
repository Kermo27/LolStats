using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LolStatsTracker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileIconAndRank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfileIconId",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoloLP",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoloRank",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoloTier",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileIconId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SoloLP",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SoloRank",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SoloTier",
                table: "UserProfiles");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LolStatsTracker.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameLaneFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Old Support (ally support for ADC) -> New LaneAlly (ally for any role)
            migrationBuilder.RenameColumn(
                name: "Support",
                table: "Matches",
                newName: "LaneAlly");

            // Old EnemyBot (enemy ADC) -> New LaneEnemy (main enemy for any role)
            migrationBuilder.RenameColumn(
                name: "EnemyBot",
                table: "Matches",
                newName: "LaneEnemy");

            // Old EnemySupport -> New LaneEnemyAlly (enemy's ally for any role)
            migrationBuilder.RenameColumn(
                name: "EnemySupport",
                table: "Matches",
                newName: "LaneEnemyAlly");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LaneAlly",
                table: "Matches",
                newName: "Support");

            migrationBuilder.RenameColumn(
                name: "LaneEnemy",
                table: "Matches",
                newName: "EnemyBot");

            migrationBuilder.RenameColumn(
                name: "LaneEnemyAlly",
                table: "Matches",
                newName: "EnemySupport");
        }
    }
}

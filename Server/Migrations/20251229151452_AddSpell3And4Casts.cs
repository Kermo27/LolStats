using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LolStatsTracker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSpell3And4Casts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Spell3Casts",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Spell4Casts",
                table: "Matches",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Spell3Casts",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Spell4Casts",
                table: "Matches");
        }
    }
}

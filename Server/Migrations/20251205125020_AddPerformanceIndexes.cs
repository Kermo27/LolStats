using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LolStatsTracker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Matches_Date",
                table: "Matches",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_ProfileId_Date",
                table: "Matches",
                columns: new[] { "ProfileId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Matches_Date",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_ProfileId_Date",
                table: "Matches");
        }
    }
}
